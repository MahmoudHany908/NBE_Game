/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IOPath = System.IO.Path;

namespace Microsoft.Unity.VisualStudio.Editor
{
    internal class CodeBuddyCNInstallation : CodeEditorInstallation
    {
        protected override string EditorName => "CodeBuddy CN";
        protected override string ExecutableName => "codebuddy-cn";
        protected override string WindowsExecutablePattern => ".*CodeBuddy CN.*\\.exe$";
        protected override string macOSAppPattern => ".*CodeBuddy CN.*\\.app$";
        protected override string LinuxDesktopFileName => "codebuddy-cn.desktop";

        protected override CodeEditorInstallation CreateInstallationInstance() => new CodeBuddyCNInstallation();

        protected override List<string> GetDefaultPaths()
        {
            var candidates = new List<string>();

#if UNITY_EDITOR_WIN
            var localAppPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs");
            var programFiles = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

            foreach (var basePath in new[] { localAppPath, programFiles })
            {
                candidates.Add(IOPath.Combine(basePath, "CodeBuddy CN", "CodeBuddy CN.exe"));
            }
#elif UNITY_EDITOR_OSX
            var appPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            candidates.AddRange(Directory.EnumerateDirectories(appPath, "CodeBuddy CN*.app"));
#elif UNITY_EDITOR_LINUX
            candidates.Add("/usr/bin/codebuddy-cn");
            candidates.Add("/bin/codebuddy-cn");
            candidates.Add("/usr/local/bin/codebuddy-cn");
            candidates.AddRange(GetXdgCandidates());
#endif

            return candidates;
        }

        public static bool TryDiscoverInstallation(string editorPath, out IVisualStudioInstallation installation)
        {
            return new CodeBuddyCNInstallation().TryDiscoverInstallationInternal(editorPath, out installation);
        }

        public static IEnumerable<IVisualStudioInstallation> GetInstallations()
        {
            return new CodeBuddyCNInstallation().GetInstallationsInternal();
        }

        public new static void Initialize()
        {
            CodeEditorInstallation.Initialize();
        }
    }
}
