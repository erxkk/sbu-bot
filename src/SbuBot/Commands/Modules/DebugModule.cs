using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Views;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    [RequireBotOwner]
    [Description("A collection of commands for debugging and testing.")]
    public sealed partial class DebugModule : SbuModuleBase
    {
        [Command("echo")]
        [Description(
            "Removes the original message and replies with the given message in the given target channel."
        )]
        public async Task EchoAsync(
            [Description("The target channel in which to send the message in.")]
            ITextChannel target,
            [Description("The message to reply with.")]
            string message = "echo!"
        )
        {
            await Context.Message.DeleteAsync();
            await target.SendMessageAsync(new LocalMessage().WithContent(message));
        }

        [Group("do")]
        [Description("A group of commands that invoke other commands with a proxy context.")]
        public sealed class ProxySubModule : SbuModuleBase
        {
            [Command]
            [Description(
                "Sends a given proxy command, or `ping` if not specified, as a given author in a given channel."
            )]
            public void Do(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            ) => Context.Bot.Queue.Post(
                new DiscordGuildCommandContext(
                    Context.Bot,
                    Context.Prefix,
                    command,
                    new ProxyMessage(Context.Message, command, member, channel.Id),
                    (channel as CachedTextChannel) ?? Context.Channel,
                    Context.Services
                ),
                context => context.Bot.ExecuteAsync(context)
            );

            [Command("as")]
            [Description("Sends a given proxy command, or `ping` if not specified, as a given author.")]
            public void DoAsUser(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy command.")] string command = "ping"
            ) => Do(member, Context.Channel, command);

            [Command("in")]
            [Description("Sends a given proxy command, or `ping` if not specified, in a given channel.")]
            public void DoInChannel(
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            ) => Do(Context.Author, channel, command);
        }

        [Command("lock")]
        [Description("Sets the bot lock state to the given state, or switches it if no state is specified.")]
        public DiscordCommandResult Lock(
            [OverrideDefault("{!state}")][Description("The new lock state to set the bot to.")]
            bool? set = null
        )
        {
            SbuBot bot = (Context.Bot as SbuBot)!;
            bot.IsLocked = set ?? !bot.IsLocked;
            return Reply($"{(bot.IsLocked ? "Locked" : "Unlocked")} the bot.");
        }

        [Command("toggle")]
        [Description("Disables/Enables a given command or module.")]
        public DiscordCommandResult Toggle(
            [Description("The command or module to disable/enable.")]
            string query
        )
        {
            IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(query);

            bool wasDisabled = false;

            // TODO: similar naming will always bring up the same submodule
            switch (matches.Count)
            {
                case 0 when Context.Bot.Commands.GetAllModules().FirstOrDefault(m => m.FullAliases.Contains(query))
                    is { } module:
                {
                    if (module.IsEnabled)
                    {
                        module.Disable();
                    }
                    else
                    {
                        wasDisabled = true;
                        module.Enable();
                    }

                    return Reply($"{(wasDisabled ? "Enabled" : "Disabled")} module.");
                }

                case 0:
                    return Reply("No matching command or module found.");

                case 1:
                {
                    if (matches[0].Command.IsEnabled)
                    {
                        matches[0].Command.Disable();
                    }
                    else
                    {
                        wasDisabled = true;
                        matches[0].Command.Enable();
                    }

                    return Reply($"{(wasDisabled ? "Enabled" : "Disabled")} command.");
                }

                default:
                    return Reply("More than one matching command or module found.");
            }
        }

        [Command("chunk")]
        [Description("Chunks the current guild.")]
        public async Task<DiscordCommandResult> Chunk()
        {
            await using (_ = Context.BeginYield())
            {
                await Context.Bot.Chunker.ChunkAsync(Context.Guild);
            }

            return Reply("Chunking finished.");
        }

        [Command("kill")]
        [Description("Fucking kills the bot oh my god...")]
        public async Task Kill()
        {
            await Reply("Gn kid.");
            Environment.Exit(1);
        }

        [Command("test")]
        [Description("A test command.")]
        public async Task<DiscordCommandResult> TestAsync()
        {
            // return FilledPages(Enumerable.Range(1, 9).Select(i => i.ToString()), 3);
            // return HelpView(Context.Bot.Commands.GetAllCommands().First(c => c.Aliases.Contains("as")));
            // return Reply(new LocalEmbed().WithDescription(Markdown.CodeBlock("yml", tag.GetInspection(3))));

            // const SomeEnum enu = SomeEnum.Value1 | SomeEnum.Value2 | SomeEnum.Value3;
            // return Reply(enu.GetInspection(3));
            ConfirmationState a = await ConfirmationAsync();
            return Response($"some response, result: {a}");
        }

        // [Flags]
        // private enum SomeEnum
        // {
        //     Value1,
        //     Value2,
        //     Value3,
        //     Value4,
        // }
    }
}