﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AHpx.Extensions.StringExtensions;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Modules;
using Mirai.Net.Modules;
using Mirai.Net.Sessions;

namespace Mirai.Net.Utils.Extensions
{
    public static class CommandExtensions
    {
        /// <summary>
        /// 根据传进来的文本判断是不是可以执行的命令
        /// </summary>
        /// <param name="module"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool CanExecute(this ICommandModule module, string s)
        {
            var method = module.GetType().GetMethod(nameof(module.Execute));
            var trigger = method!.GetCustomAttribute<CommandTriggerAttribute>();

            if (trigger == null) throw new Exception("没有添加Trigger Attribute");
                            
            var command = $"{trigger.Prefix}{trigger.Name}";
            var predicate = new Predicate<string>(s => s.Contains(command));

            if (trigger.EqualName) predicate = s => s == command;

            return s.Split(' ').Any(predicate.Invoke);
        }

        /// <summary>
        /// 执行集合内的全部命令模块
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="modules"></param>
        /// <param name="bot"></param>
        public static void ExecuteCommands(this MessageReceiverBase receiver, IEnumerable<ICommandModule> modules, MiraiBot bot)
        {
            foreach (var message in receiver.MessageChain)
            {
                foreach (var module in modules)
                {
                    if (message is PlainMessage plainMessage)
                    {
                        if (module.CanExecute(plainMessage.Text))
                        {
                            module.Execute(bot, receiver, message);
                        }
                    }
                    else
                    {
                        if (module.CanExecute(message.ToJsonString()))
                        {
                            module.Execute(bot, receiver, message);
                        }
                    }
                }
            }
        }
    }
}