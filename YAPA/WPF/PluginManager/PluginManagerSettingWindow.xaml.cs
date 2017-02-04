﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YAPA.Contracts;
using YAPA.Shared;

namespace YAPA.WPF
{
    public partial class PluginManagerSettingWindow : UserControl
    {
        private readonly PluginManagerSettings _settings;
        private readonly ISettingManager _settingManager;
        public List<PluginViewModel> Plugins { get; set; }
        public ICommand TogglePlugin { get; set; }
        private readonly List<string> _disabledPlugins;

        public PluginManagerSettingWindow(IPluginManager plugins, PluginManagerSettings settings, ISettings globalSettings, ISettingManager settingManager)
        {
            _settings = settings;
            _settings.DeferChanges();

            _settingManager = settingManager;
            globalSettings.PropertyChanged += _globalSettings_PropertyChanged;

            _disabledPlugins = _settings.DisabledPlugins;

            Plugins = new List<PluginViewModel>();
            foreach (var pluginMeta in plugins.CustomPlugins)
            {
                Plugins.Add(new PluginViewModel
                {
                    Title = pluginMeta.Title,
                    Enabled = !_disabledPlugins.Contains(pluginMeta.Title)
                });
            }

            InitializeComponent();

            PluginList.ItemsSource = Plugins;
            DataContext = this;
        }

        private void _globalSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == $"{nameof(PluginManager)}.{nameof(_settings.DisabledPlugins)}")
            {
                _settingManager.RestartNeeded = true;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var context = ((CheckBox)sender).DataContext as PluginViewModel;
            if (context.Enabled)
            {
                var existing = _disabledPlugins.FirstOrDefault(x => x == context.Title);
                if (existing != null)
                {
                    _disabledPlugins.Remove(existing);
                }
            }
            else
            {
                _disabledPlugins.Add(context.Title);
            }

            if (_disabledPlugins.Count == 0)
            {
                _settings.DisabledPlugins = null;
            }
            else
            {
                _settings.DisabledPlugins = _disabledPlugins;
            }
        }
    }

    public class PluginViewModel
    {
        public string Title { get; set; }
        public bool Enabled { get; set; }
    }

}
