﻿using Discord;
using Discord.Commands;
using OniBot.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class DebugCommands : ModuleBase, IBotCommand
    {
        [Command("dumpbot", RunMode = RunMode.Async)]
        [Summary("Gets the current run state of the bot")]
        [RequireOwner]
        public async Task DumpMyself()
        {

            var userDmChannel = await Context.User.CreateDMChannelAsync();
            var props = DumpProps(Context.Client.CurrentUser);
            await userDmChannel.SendMessageAsync(props);
        }

        [Command("dumpuser")]
        [Summary("Gets the current run state of a user")]
        [RequireOwner]
        public async Task DumpUser([Remainder] string user)
        {
            var users = await Context.Guild.GetUsersAsync();
            var userDmChannel = await Context.User.CreateDMChannelAsync();
            var selectedUser = users.SingleOrDefault(a => a.Mention == user || a.Mention == user.Replace("<@", "<@!"));
            if (selectedUser == null)
            {
                return;
            }

            var props = DumpProps(selectedUser);
            await userDmChannel.SendMessageAsync(props);
        }

        [Command("dumpchat")]
        [Summary("Gets the current run state of a user")]
        [RequireOwner]
        public async Task DumpChat([Remainder] string count)
        {
            var amount = int.Parse(count);

            var messages = await Context.Channel.GetMessagesAsync(limit: amount, fromMessageId: Context.Message.Id, dir: Direction.Before).ToList();
            var userDmChannel = await Context.User.CreateDMChannelAsync();
            foreach (var messageContainer in messages)
            {
                foreach (var message in messageContainer)
                {
                    var props = DumpProps(message);

                    await userDmChannel.SendMessageAsync(props);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        private string DumpProps(object property)
        {
            var sb = new StringBuilder();
            var userType = property.GetType();
            sb.AppendLine($"```{"Type".PadRight(20)}{userType}");
            var props = userType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in props)
            {

                var key = prop.Name.PadRight(20);
                object value;
                try
                {
                    value = prop.GetValue(property);

                }
                catch (Exception ex)
                {
                    value = $"Exception while processing property value: {ex.Message}";
                }
                sb.AppendLine($"{key}{value}");
            }

            sb.Append("```");
            return sb.ToString();
        }
    }
}