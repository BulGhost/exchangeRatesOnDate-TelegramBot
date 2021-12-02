using ExchangeRatesOnDate.Bot;
using ExchangeRatesOnDate.ExtensionsWrapper;
using ExchangeRatesOnDate.Resources;
using ExchangeRatesOnDate.Tests.Stubs;
using FreeCurrencyExchangeApiLib;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Xunit;

namespace ExchangeRatesOnDate.Tests
{
    public class HandlersTests
    {
        private readonly Handlers _handlers;
        private readonly Mock<ICurrencyExchanger> _currencyExchangerMock = new();
        private readonly Mock<IExtensionsWrapper> _extensionsWrapperMock = new();
        private readonly Mock<ITelegramBotClient> _botClientMock = new();
        private readonly Mock<ILogger> _loggerMock = new();

        public static IEnumerable<object[]> UpdatesNotMessages =>
            new List<object[]>
            {
                new object[] { new Update { InlineQuery = new InlineQuery()} },
                new object[] { new Update { ChosenInlineResult = new ChosenInlineResult()} },
                new object[] { new Update { CallbackQuery = new CallbackQuery()} },
                new object[] { new Update { ChannelPost = new Message()} },
                new object[] { new Update { Poll = new Poll()} }
            };

        public static IEnumerable<object[]> MessagesWithNotTextData =>
            new List<object[]>
            {
                new object[] { new Message { Chat = new Chat(), Voice = new Voice()} },
                new object[] { new Message { Chat = new Chat(), Sticker = new Sticker() } },
                new object[] { new Message { Chat = new Chat(), Document = new Document()} }
            };

        public static IEnumerable<object[]> MessagesWithInvalidTextData =>
            new List<object[]>
            {
                new object[] { new Message { Chat = new Chat(), Text = "qwerty"} },
                new object[] { new Message { Chat = new Chat(), Text = "123" } },
                new object[] { new Message { Chat = new Chat(), Text = "dollar 03.04.2021" } }
            };

        public HandlersTests()
        {
            _handlers = new Handlers(_currencyExchangerMock.Object, _extensionsWrapperMock.Object, _loggerMock.Object);
        }

        [Theory]
        [MemberData(nameof(UpdatesNotMessages))]
        public void Do_nothing_when_received_update_not_of_type_Message(Update update)
        {
            Task task = _handlers.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _botClientMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MessagesWithNotTextData))]
        public void Send_error_message_to_client_when_received_message_with_not_text_data(Message message)
        {
            var updateStub = new Update { Message = message };

            Task task = _handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, message.Chat.Id, TextResources.NotSupportedCommand, default, default, default, default, default, default, default, default),
                Times.Once);
            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, message.Chat.Id, TextResources.Instruction, default, default, default, default, default, default, default, default),
                Times.Once);
        }

        [Theory]
        [MemberData(nameof(MessagesWithInvalidTextData))]
        public void Send_error_message_to_client_when_received_message_invalid_text_data(Message message)
        {
            var updateStub = new Update { Message = message };

            Task task = _handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, message.Chat.Id, TextResources.UnknownCurrencyCode, default, default, default, default, default, default, default, default),
                Times.Once);
            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, message.Chat.Id, TextResources.Instruction, default, default, default, default, default, default, default, default),
                Times.Once);
        }

        [Theory]
        [InlineData("USD 31.02.2020")]
        [InlineData("EUR 195")]
        [InlineData("JPY asd")]
        public void Send_error_message_to_client_when_received_request_with_invalid_date(string message)
        {
            var chat = new Chat();
            var updateStub = new Update { Message = new Message { Chat = chat, Text = message } };

            Task task = _handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, TextResources.InvalidDate, default, default, default, default, default, default, default, default),
                Times.Once);
            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, TextResources.Instruction, default, default, default, default, default, default, default, default),
                Times.Once);
        }

        [Fact]
        public void Send_error_message_to_client_when_received_request_with_date_from_future()
        {
            var chat = new Chat();
            string message = "USD " + DateTime.Now.AddDays(1);
            var updateStub = new Update { Message = new Message { Chat = chat, Text = message } };

            Task task = _handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, TextResources.DateInFuture, default, default, default, default, default, default, default, default),
                Times.Once);
            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, TextResources.Instruction, default, default, default, default, default, default, default, default),
                Times.Once);
        }

        [Fact]
        public void Send_info_message_to_client_when_no_data_available_on_request()
        {
            const string exceptionMessage = "exMessage";
            _currencyExchangerMock.Setup(mock =>
                    mock.DetermineExchangeRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Throws(new ArgumentException(exceptionMessage));
            var chat = new Chat();
            var updateStub = new Update { Message = new Message { Chat = chat, Text = "BTC 16.05.2021" } };
            var handlers = new Handlers(_currencyExchangerMock.Object, _extensionsWrapperMock.Object, _loggerMock.Object);

            Task task = handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, exceptionMessage, default, default, default, default, default, default, default, default),
                Times.Once);
        }

        [Fact]
        public void Send_info_message_to_client_if_library_used_threw_HttpRequestException()
        {
            _currencyExchangerMock.Setup(mock =>
                    mock.DetermineExchangeRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Throws<HttpRequestException>();
            var chat = new Chat();
            var updateStub = new Update { Message = new Message { Chat = chat, Text = "USD 16.05.2021" } };
            var handlers = new Handlers(_currencyExchangerMock.Object, _extensionsWrapperMock.Object, _loggerMock.Object);

            Task task = handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, TextResources.HttpRequestFail, default, default, default, default, default, default, default, default),
                Times.Once);
        }

        [Theory]
        [InlineData("USD 16.05.2021", 0.01418, "16.05.2021, 1 USD =  70,52 RUB")]
        [InlineData("KZT 22.07.2018", 5.217, "22.07.2018, 1 KZT =  0,192 RUB")]
        public void Send_exchange_rates_on_date_as_requested(string request, decimal exchangeRate, string response)
        {
            _currencyExchangerMock.Setup(mock =>
                    mock.DetermineExchangeRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(exchangeRate);
            var chat = new Chat();
            var updateStub = new Update { Message = new Message { Chat = chat, Text = request } };
            var handlers = new Handlers(_currencyExchangerMock.Object, _extensionsWrapperMock.Object, _loggerMock.Object);

            Task task = handlers.HandleUpdateAsync(_botClientMock.Object, updateStub, CancellationToken.None);
            if (!task.IsCompleted)
            {
                task.RunSynchronously();
            }

            _extensionsWrapperMock.Verify(wrapper => wrapper.SendTextMessageAsync(_botClientMock.Object, chat.Id, response, default, default, default, default, default, default, default, default),
                Times.Once);
        }
    }
}
