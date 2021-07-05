using System;
using System.Collections.Generic;

using Disqord;
using Disqord.Gateway;
using Disqord.Models;

namespace SbuBot.Commands
{
    public sealed class ProxyMessage : IGatewayUserMessage
    {
        private readonly IGatewayUserMessage _originalGatewayUserMessage;

        public IUser Author { get; }
        public string Content { get; }
        public Snowflake ChannelId { get; }

        public IReadOnlyList<IUser> MentionedUsers => _originalGatewayUserMessage.MentionedUsers;

        public Optional<IReadOnlyDictionary<IEmoji, MessageReaction>> Reactions
            => _originalGatewayUserMessage.Reactions;

        public IClient Client => _originalGatewayUserMessage.Client;
        public Snowflake Id => _originalGatewayUserMessage.Id;
        public Snowflake? GuildId => _originalGatewayUserMessage.GuildId;
        public DateTimeOffset? EditedAt => _originalGatewayUserMessage.EditedAt;
        public Snowflake? WebhookId => _originalGatewayUserMessage.WebhookId;
        public bool IsTextToSpeech => _originalGatewayUserMessage.IsTextToSpeech;
        public Optional<string> Nonce => _originalGatewayUserMessage.Nonce;
        public bool IsPinned => _originalGatewayUserMessage.IsPinned;
        public bool MentionsEveryone => _originalGatewayUserMessage.MentionsEveryone;
        public IReadOnlyList<Snowflake> MentionedRoleIds => _originalGatewayUserMessage.MentionedRoleIds;
        public IReadOnlyList<Attachment> Attachments => _originalGatewayUserMessage.Attachments;
        public IReadOnlyList<Embed> Embeds => _originalGatewayUserMessage.Embeds;
        public MessageActivity Activity => _originalGatewayUserMessage.Activity;
        public MessageApplication Application => _originalGatewayUserMessage.Application;
        public MessageReference Reference => _originalGatewayUserMessage.Reference;
        public MessageFlag Flags => _originalGatewayUserMessage.Flags;
        public IReadOnlyList<MessageSticker> Stickers => _originalGatewayUserMessage.Stickers;
        public Optional<IUserMessage> ReferencedMessage => _originalGatewayUserMessage.ReferencedMessage;
        public IReadOnlyList<IComponent> Components => _originalGatewayUserMessage.Components;

        internal ProxyMessage(
            IGatewayUserMessage originalGatewayUserMessage,
            string? proxyContent = null,
            IUser? proxyAuthor = null,
            Snowflake? proxyChannelId = null
        )
        {
            _originalGatewayUserMessage = originalGatewayUserMessage;
            Author = proxyAuthor ?? originalGatewayUserMessage.Author;
            Content = proxyContent ?? originalGatewayUserMessage.Content;
            ChannelId = proxyChannelId ?? originalGatewayUserMessage.ChannelId;
        }

        public void Update(MessageJsonModel model)
            => throw new InvalidOperationException("This proxy message should not be updated");
    }
}