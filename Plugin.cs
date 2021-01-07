﻿using System;
using ImGuiNET;

using Dalamud.Plugin;
using DeepDungeonDex.Attributes;
using Dalamud.Game.Internal;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.Command;

namespace DeepDungeonDex
{
    public class Plugin : IDalamudPlugin
    {
        private DalamudPluginInterface pluginInterface;
        private Configuration config;
        private PluginUI ui;
        private ConfigUI cui;
        public float Opacity { get; set; }
        public bool IsClickthrough { get; set; }
        private Actor previousTarget;

        public string Name => "DeepDungeonDex";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.config = (Configuration)this.pluginInterface.GetPluginConfig() ?? new Configuration();
            Opacity = config.Opacity;
            IsClickthrough = config.IsClickthrough;
            this.config.Initialize(this.pluginInterface);


            this.ui = new PluginUI();
            this.cui = new ConfigUI();
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;
            this.pluginInterface.UiBuilder.OnBuildUi += this.cui.Draw;

            this.pluginInterface.CommandManager.AddHandler("/pddd", new CommandInfo(OpenConfig)
            {
                HelpMessage = "DeepDungeonDex config"
            });

            this.pluginInterface.Framework.OnUpdateEvent += this.GetData;
        }

        public void OpenConfig(string command, string args)
        {
            cui.Initialize(this.pluginInterface);
            cui.IsVisible = true;
        }

        public void GetData(Framework framework)
        {
            //var chat = this.pluginInterface.Framework.Gui.Chat;
            if (!this.pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InDeepDungeon]) return;
            var target = pluginInterface.ClientState.Targets.CurrentTarget;
            if (target == null || target == previousTarget) 
            {
                ui.IsVisible = false;
                return;
            }
            TargetData t = new TargetData();
            if (!t.IsValidTarget(target))
            {
                ui.IsVisible = false;
                return;
            }
            else
            { 
                previousTarget = target;
                ui.IsVisible = true;
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.pluginInterface.CommandManager.RemoveHandler("/pddd");

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;

            this.pluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}