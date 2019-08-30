// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.CL
{
    /// <summary>
    /// Interface of console top-level commands.
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Redirect function.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns>Whether the redirect was successful.</returns>
        bool Redirect(string[] arguments, string contents);
    }

    /// <summary>
    /// This is a console option that must be included.
    /// </summary>
    public class IConsoleOption
    {
        public bool Error;
        public string ErrorMessage;
        public string HelpMessage;
    }
}
