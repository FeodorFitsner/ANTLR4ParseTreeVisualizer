﻿using ParseTreeVisualizer.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static System.IO.Path;

namespace ParseTreeVisualizer {
    public class ConfigViewModel : ViewModelBase<Config> {
        readonly Config _originalValues;
        public ConfigViewModel(Config config, VisualizerData visualizerData) : 
            this(config, visualizerData.TokenTypeMapping, visualizerData.UsedRuleContexts, visualizerData.AvailableLexers, visualizerData.AvailableParsers) { }

        public ConfigViewModel(Config config, Dictionary<int, string> tokenTypeMapping, List<ClassInfo> ruleContexts, List<ClassInfo> lexers, List<ClassInfo> parsers) : base(config.Clone()) {
            TokenTypes = tokenTypeMapping?.SelectKVP((index, text) => {
                var ret = new TokenTypeViewModel(index, text) {
                    IsSelected = index.In(Model.SelectedTokenTypes)
                };
                ret.PropertyChanged += (s, e) => {
                    if (e.PropertyName == "IsSelected") {
                        Model.SelectedTokenTypes.AddRemove(ret.IsSelected, ret.Index);
                    }
                };
                return ret;
            }).OrderBy(x => x.Text).ToList().AsReadOnly();

            RuleContexts = ruleContexts.Select(x => {
                var ret = new Selectable<ClassInfo>(x) {
                    IsSelected = x.FullName.In(Model.SelectedRuleContexts)
                };
                ret.PropertyChanged += (s, e) => {
                    if (e.PropertyName == "IsSelected") {
                        Model.SelectedRuleContexts.AddRemove(ret.IsSelected, x.FullName);
                    }
                };
                return ret;
            }).ToList().AsReadOnly();

            AvailableLexers = getVMList(lexers);
            AvailableParsers = getVMList(parsers);

            _originalValues = config;
        }

        private ReadOnlyCollection<Selectable<ClassInfo>> getVMList(List<ClassInfo> models) {
            var lst = models.Select(x => new Selectable<ClassInfo>(x) {
                IsSelected = Model.SelectedParserName == x.FullName
            }).OrderBy(x => x.Model.Name).ToList();
            lst.Insert(0, new Selectable<ClassInfo>(ClassInfo.None));
            return lst.AsReadOnly();
        }

        public ReadOnlyCollection<TokenTypeViewModel> TokenTypes { get; }
        public ReadOnlyCollection<Selectable<ClassInfo>> RuleContexts { get; }
        public ReadOnlyCollection<Selectable<ClassInfo>> AvailableParsers { get; }
        public ReadOnlyCollection<Selectable<ClassInfo>> AvailableLexers { get; }

        public string Version => GetType().Assembly.GetName().Version.ToString();
        public string Location => GetType().Assembly.Location;
        public string Filename => GetFileName(Location);

        public bool IsDirty {
            get {
                var m = Model;
                var o = _originalValues;
                return
                    o.SelectedParserName != m.SelectedParserName ||
                    o.SelectedLexerName != m.SelectedLexerName ||
                    o.ShowTextTokens != m.ShowTextTokens ||
                    o.ShowErrorTokens != m.ShowErrorTokens ||
                    o.ShowWhitespaceTokens != m.ShowWhitespaceTokens ||
                    o.ShowTreeErrorTokens != m.ShowTreeErrorTokens ||
                    o.ShowTreeTextTokens != m.ShowTreeTextTokens ||
                    o.ShowTreeWhitespaceTokens != m.ShowTreeWhitespaceTokens ||
                    o.ShowRuleContextNodes != m.ShowRuleContextNodes ||

                    !o.SelectedTokenTypes.SetEquals(m.SelectedTokenTypes) ||
                    !o.SelectedRuleContexts.SetEquals(m.SelectedRuleContexts);
            }
        }
    }
}
