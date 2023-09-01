﻿using AasxServer;
using AasxServerStandardBib.Logging;
using Extensions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

namespace AasSecurity
{
    /**
     * This class initiates the security from Program.cs
     */
    public static class SecurityHelper
    {
        //private static ILogger _logger = ApplicationLogging.CreateLogger("SecurityHelper");

        public static void SecurityInit()
        {
            GlobalSecurityVariables.WithAuthentication = !Program.noSecurity;
            //_logger.LogInformation($"IsSecurityEnabled: {GlobalSecurityVariables.WithAuthentication}");
            ParseSecurityMetamodel();
        }

        private static void ParseSecurityMetamodel()
        {
            foreach (var env in Program.env)
            {
                if (env != null && env.AasEnv != null && env.AasEnv.AssetAdministrationShells != null)
                {
                    foreach (var aas in env.AasEnv.AssetAdministrationShells)
                    {
                        if (aas != null && aas.Submodels != null)
                        {
                            foreach (var submodelReference in aas.Submodels)
                            {
                                var submodel = env.AasEnv.FindSubmodel(submodelReference);
                                if (submodel != null && !string.IsNullOrEmpty(submodel.IdShort))
                                {
                                    switch (submodel.IdShort.ToLower())
                                    {
                                        case "securitysettingsforserver":
                                            {
                                                //_logger.LogDebug($"Parsing the submodel {submodel.IdShort}");
                                                SecuritySettingsForServerParser.ParseSecuritySettingsForServer(env, submodel);
                                            }
                                            break;
                                        case "securitymetamodelforaas":
                                        case "securitymetamodelforserver":
                                            {
                                                //_logger.LogDebug($"Parsing the submodel {submodel.IdShort}");
                                                SecurityMetamodelParser.ParserSecurityMetamodel(env, submodel);
                                            }
                                            break;
                                        default:
                                            {

                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static X509Certificate2? FindServerCertificate(string serverName)
        {
            if (GlobalSecurityVariables.ServerCertFileNames != null)
            {
                for (int i = 0; i < GlobalSecurityVariables.ServerCertFileNames.Count; i++)
                {
                    if (Path.GetFileName(GlobalSecurityVariables.ServerCertFileNames[i]) == serverName + ".cer")
                    {
                        return GlobalSecurityVariables.ServerCertificates[i];
                    }
                }
            }

            return null;
        }
    }
}
