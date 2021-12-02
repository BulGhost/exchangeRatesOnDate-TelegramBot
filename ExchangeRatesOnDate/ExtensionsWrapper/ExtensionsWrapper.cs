using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExchangeRatesOnDate.ExtensionsWrapper
{
    internal class ExtensionsWrapper : IExtensionsWrapper
    {
        public Task SendChatActionAsync(ITelegramBotClient botClient, ChatId chatId, ChatAction chatAction,
            CancellationToken cancellationToken = default)
        {
            return botClient.SendChatActionAsync(chatId, chatAction, cancellationToken);
        }

        public Task<Message> SendTextMessageAsync(ITelegramBotClient botClient, ChatId chatId, string text, ParseMode? parseMode = null,
            IEnumerable<MessageEntity>? entities = null, bool? disableWebPagePreview = null, bool? disableNotification = null,
            int? replyToMessageId = null, bool? allowSendingWithoutReply = null, IReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            return botClient.SendTextMessageAsync(chatId, text, parseMode, entities, disableWebPagePreview,
                disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
        }
    }
}
