﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using Microsoft.Windows.Powershell.ScriptAnalyzer;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidPositionalParameters: Check to make sure that positional parameters are not used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidPositionalParameters : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyze the ast to check that positional parameters are not used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all CommandAsts.
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            // Iterrates all CommandAsts and check the command name.
            foreach (Ast foundAst in foundAsts)
            {
                CommandAst cmdAst = (CommandAst)foundAst;
                // Handles the exception caused by commands like, {& $PLINK $args 2> $TempErrorFile}.
                // You can also review the remark section in following document,
                // MSDN: CommandAst.GetCommandName Method
                if (cmdAst.GetCommandName() == null) continue;
                
                if (Helper.Instance.GetCommandInfo(cmdAst.GetCommandName()) != null
                    && Helper.Instance.PositionalParameterUsed(cmdAst))
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersError, cmdAst.GetCommandName()),
                        cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingPositionalParametersName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
