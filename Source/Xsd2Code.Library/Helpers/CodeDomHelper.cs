//-----------------------------------------------------------------------
// <copyright file="CodeDomHelper.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code.Library.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.CodeDom;

    /// <summary>
    /// Helper for create codeDom object
    /// </summary>
    internal static class CodeDomHelper
    {
        /// <summary>
        /// Add to CodeCommentStatementCollection summary documentation
        /// </summary>
        /// <param name="codeStatmentColl">Collection of CodeCommentStatement</param>
        /// <param name="comment">summary text</param>
        internal static void CreateSummaryComment(CodeCommentStatementCollection codeStatmentColl, string comment)
        {
            codeStatmentColl.Add(new CodeCommentStatement("<summary>", true));
            string[] lines = comment.Split(new char[] { '\n' });
            foreach (string line in lines)
            {
                codeStatmentColl.Add(new CodeCommentStatement(line.Trim(), true));
            }

            codeStatmentColl.Add(new CodeCommentStatement("</summary>", true));
        }
    }
}
