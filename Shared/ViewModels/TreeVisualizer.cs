﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParseTreeVisualizer.Util;
using static System.Linq.Enumerable;

namespace ParseTreeVisualizer.ViewModels {
    [Serializable]
    public class TreeVisualizer {
        public string Source { get; }
        public Config Config { get; }
        public ParseTreeNode Root { get; }
        public TokenList Tokens { get; } = new TokenList();

        public TreeVisualizer(IParseTree tree, Config config) {
            Source = tree.GetText();
            Config = config;

            string[] ruleNames = null;
            Dictionary<int, string> tokenTypeMapping = null;

            if (!config.SelectedParserName.IsNullOrWhitespace()) {
                var parserType = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType(config.SelectedParserName)).FirstOrDefault(x => x != null);
                var vocabulary = parserType.GetField("DefaultVocabulary").GetValue(null) as IVocabulary;
                tokenTypeMapping = Range(1, vocabulary.MaxTokenType).ToDictionary(x => (x, vocabulary.GetSymbolicName(x)));

                ruleNames = parserType.GetField("ruleNames").GetValue(null) as string[];
            }

            var rulenameMapping = new Dictionary<string, string>();
            Root = new ParseTreeNode(tree, Tokens, ruleNames, tokenTypeMapping, config, rulenameMapping);

            #region Load debuggee state

            if (tokenTypeMapping == null) {
                tokenTypeMapping = Range(1, Tokens.MaxTokenTypeID()).ToDictionary(x => (x, x.ToString()));
            }
            config.TokenTypeMapping = tokenTypeMapping;

            {
                // load available parsers and lexers

                var baseTypes = new[] { typeof(Parser), typeof(Lexer) };
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x != GetType().Assembly)
                    .SelectMany(x => {
                        try {
                            return x.GetTypes();
                        } catch {
                            AssemblyLoadErrors.Add(x.FullName);
                            return Empty<Type>();
                        }
                    })
                    .Where(x => !x.IsAbstract);
                foreach (var t in types) {
                    var dest =
                        t.InheritsFromOrImplements<Parser>() ? AvailableParsers :
                        t.InheritsFromOrImplements<Lexer>() ? AvailableLexers :
                        null;

                    if (dest != null) {
                        IEnumerable<ClassInfo> relatedTypes = null;
                        if (t.InheritsFromOrImplements<Parser>()) {
                            relatedTypes = t.GetNestedTypes()
                                .Where(x => x.InheritsFromOrImplements<ParserRuleContext>())
                                .Select(x => {
                                    rulenameMapping.TryGetValue(x.FullName, out var ruleName);
                                    return new ParseRuleContextInfo(x, ruleName);
                                });
                        }
                        dest.Add(new ClassInfo(t, relatedTypes));
                    }
                }

                Comparison<ClassInfo> comparison = (x, y) => string.Compare(x.Name, y.Name);
                AvailableParsers.Sort(comparison);
                AvailableLexers.Sort(comparison);

                Config.SelectedParserName = fixList(AvailableParsers, Config.SelectedParserName);
                Config.SelectedLexerName = fixList(AvailableLexers, Config.SelectedLexerName);
            }

            #endregion
        }

        #region Debuggee state
        public List<ClassInfo> AvailableParsers { get; } = new List<ClassInfo>();
        public List<ClassInfo> AvailableLexers { get; } = new List<ClassInfo>();
        public List<string> AssemblyLoadErrors { get; } = new List<string>();
        #endregion

        private string fixList(List<ClassInfo> lst, string selected) {
            lst.Insert(0, ClassInfo.None);
            if (lst.None(x => x.FullName == selected)) {
                selected = null;
            }
            if (selected.IsNullOrWhitespace()) {
                selected = (
                    lst.OneOrDefault(x => x.Antlr != "Runtime") ??
                    lst.OneOrDefault()
                )?.FullName;
            }
            return selected;
        }
    }
}
