/********************************************************************************
* Copyright (c) {2019 - 2024} Contributors to the Eclipse Foundation
*
* See the NOTICE file(s) distributed with this work for additional
* information regarding copyright ownership.
*
* This program and the accompanying materials are made available under the
* terms of the Apache License Version 2.0 which is available at
* https://www.apache.org/licenses/LICENSE-2.0
*
* SPDX-License-Identifier: Apache-2.0
********************************************************************************/

namespace AasxServerDB
{
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Text;
    using AasCore.Aas3_0;
    using AasxServerDB.Entities;
    using AdminShellNS;
    using Contracts.Pagination;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using TimeStamp;


    public class Converter
    {
        public static List<string> GetFilteredPackages(string filterPath, List<AdminShellPackageEnv> list)
        {
            var paths = new List<string>();
            var db = new AasContext();

            var envList = db.EnvSets.Where(e => e.Path.Contains(filterPath));

            foreach (var env in envList)
            {
                var p = GetPackageEnv(env.Id);
                if (p != null)
                {
                    list.Add(p);
                    paths.Add(env.Path);
                }
            }

            return paths;
        }

        public static AdminShellPackageEnv? GetPackageEnv(int envId)
        {
            var timeStamp = DateTime.UtcNow;

            // env
            var env = new AdminShellPackageEnv();
            env.AasEnv.ConceptDescriptions = new List<IConceptDescription>();
            env.AasEnv.AssetAdministrationShells = new List<IAssetAdministrationShell>();
            env.AasEnv.Submodels = new List<ISubmodel>();

            // db
            var db = new AasContext();

            // path
            env.SetFilename(fileName: GetAASXPath(envId));

            // cd
            var cdDBList = db.EnvCDSets.Where(envcd => envcd.EnvId == envId).Join(db.CDSets, envcd => envcd.CDId, cd => cd.Id, (envcd, cd) => cd).ToList();
            foreach (var cd in cdDBList.Select(selector: cdDB => GetConceptDescription(cdDB: cdDB)))
            {
                env.AasEnv.ConceptDescriptions?.Add(cd);
            }

            // aas
            var aasDBList = db.AASSets.Where(cd => cd.EnvId == envId).ToList();
            foreach (var aasDB in aasDBList)
            {
                var aas = GetAssetAdministrationShell(aasDB: aasDB);
                if (aas.TimeStamp == DateTime.MinValue)
                {
                    aas.TimeStampCreate = timeStamp;
                    aas.SetTimeStamp(timeStamp);
                }
                env.AasEnv.AssetAdministrationShells?.Add(aas);

                // sm
                var smAASDBList = db.SMSets.Where(sm => sm.EnvId == envId && sm.AASId == aasDB.Id).ToList();
                foreach (var sm in smAASDBList.Select(selector: smDB => GetSubmodel(smDB: smDB)))
                {
                    aas.Submodels?.Add(sm.GetReference());
                }
            }

            // sm
            var smDBList = db.SMSets.Where(cd => cd.EnvId == envId).ToList();
            foreach (var sm in smDBList.Select(selector: submodelDB => GetSubmodel(smDB: submodelDB)))
            {
                if (sm.TimeStamp == DateTime.MinValue)
                {
                    sm.SetAllParentsAndTimestamps(null, timeStamp, timeStamp, DateTime.MinValue);
                    sm.SetTimeStamp(timeStamp);
                }
                env.AasEnv.Submodels?.Add(sm);
            }
            return env;
        }

        public static List<IAssetAdministrationShell> GetPagedAssetAdministrationShells(IPaginationParameters paginationParameters, List<ISpecificAssetId> assetIds)
        {
            List<IAssetAdministrationShell> output = new List<IAssetAdministrationShell>();

            /*
            var db = new AasContext();
            var timeStamp = DateTime.UtcNow;

            var aasDBList = db.AASSets.ToList();
            foreach (var aasDB in aasDBList)
            {
                int envId = aasDB.EnvId;

                var aas = GetAssetAdministrationShell(aasDB: aasDB);
                if (aas.TimeStamp == DateTime.MinValue)
                {
                    aas.TimeStampCreate = timeStamp;
                    aas.SetTimeStamp(timeStamp);
                }

                // sm
                var smAASDBList = db.SMSets.Where(sm => sm.EnvId == envId && sm.AASId == aasDB.Id).ToList();
                foreach (var sm in smAASDBList)
                {
                    aas.Submodels?.Add(new Reference(type: ReferenceTypes.ModelReference,
                        keys: new List<IKey>() { new Key(KeyTypes.Submodel, sm.Identifier) }
                        ));
                }

                output.Add(aas);
            }
            */

            using (var db = new AasContext())
            {
                var timeStamp = DateTime.UtcNow;

                var aasDBList = db.AASSets
                    .Skip(paginationParameters.Cursor)
                    .Take(paginationParameters.Limit)
                    .Include(aas => aas.SMSets) // Include related SMSets
                    .ToList();

                foreach (var aasDB in aasDBList)
                {
                    int envId = aasDB.EnvId;

                    var aas = GetAssetAdministrationShell(aasDB: aasDB);
                    if (aas.TimeStamp == DateTime.MinValue)
                    {
                        aas.TimeStampCreate = timeStamp;
                        aas.SetTimeStamp(timeStamp);
                    }

                    // sm
                    foreach (var sm in aasDB.SMSets.Where(sm => sm.EnvId == envId))
                    {
                        aas.Submodels?.Add(new Reference(type: ReferenceTypes.ModelReference,
                            keys: new List<IKey>() { new Key(KeyTypes.Submodel, sm.Identifier) }
                        ));
                    }

                    output.Add(aas);
                }
            }

            return output;
        }

        private static ConceptDescription? GetConceptDescription(CDSet? cdDB = null, string cdIdentifier = "")
        {
            var db = new AasContext();
            if (!cdIdentifier.IsNullOrEmpty())
            {
                var cdList = db.CDSets.Where(cd => cd.Identifier == cdIdentifier).ToList();
                if (cdList.Count == 0)
                    return null;
                cdDB = cdList.First();
            }

            if (cdDB == null)
                return null;

            var cd = new ConceptDescription(
                idShort:                    cdDB.IdShort,
                displayName:                Serializer.DeserializeList<ILangStringNameType>(cdDB.DisplayName),
                category:                   cdDB.Category,
                description:                Serializer.DeserializeList<ILangStringTextType>(cdDB.Description),
                extensions:                 Serializer.DeserializeList<IExtension>(cdDB.Extensions),
                id:                         cdDB.Identifier,
                isCaseOf:                   Serializer.DeserializeList<IReference>(cdDB.IsCaseOf),
                embeddedDataSpecifications: Serializer.DeserializeList<IEmbeddedDataSpecification>(cdDB.EmbeddedDataSpecifications),
                administration:             new AdministrativeInformation(
                    version:                    cdDB.Version,
                    revision:                   cdDB.Revision,
                    creator:                    Serializer.DeserializeElement<IReference>(cdDB.Creator),
                    templateId:                 cdDB.TemplateId,
                    embeddedDataSpecifications: Serializer.DeserializeList<IEmbeddedDataSpecification>(cdDB.AEmbeddedDataSpecifications)
                )
            )
            {
                TimeStampCreate = cdDB.TimeStampCreate,
                TimeStamp       = cdDB.TimeStamp,
                TimeStampTree   = cdDB.TimeStampTree,
                TimeStampDelete = cdDB.TimeStampDelete
            };

            return cd;
        }

        private static AssetAdministrationShell? GetAssetAdministrationShell(AASSet? aasDB = null, string aasIdentifier = "")
        {
            var db = new AasContext();
            if (!aasIdentifier.IsNullOrEmpty())
            {
                var aasList = db.AASSets.Where(cd => cd.Identifier == aasIdentifier).ToList();
                if (aasList.Count == 0)
                    return null;
                aasDB = aasList.First();
            }

            if (aasDB == null)
                return null;

            var aas = new AssetAdministrationShell(
                idShort:                    aasDB.IdShort,
                displayName:                Serializer.DeserializeList<ILangStringNameType>(aasDB.DisplayName),
                category:                   aasDB.Category,
                description:                Serializer.DeserializeList<ILangStringTextType>(aasDB.Description),
                extensions:                 Serializer.DeserializeList<IExtension>(aasDB.Extensions),
                id:                         aasDB.Identifier,
                embeddedDataSpecifications: Serializer.DeserializeList<IEmbeddedDataSpecification>(aasDB.EmbeddedDataSpecifications),
                derivedFrom:                Serializer.DeserializeElement<IReference>(aasDB.DerivedFrom),
                submodels: new List<IReference>(),
                administration: new AdministrativeInformation(
                    version: aasDB.Version,
                    revision: aasDB.Revision,
                    creator: Serializer.DeserializeElement<IReference>(aasDB.Creator),
                    templateId: aasDB.TemplateId,
                    embeddedDataSpecifications: Serializer.DeserializeList<IEmbeddedDataSpecification>(aasDB.AEmbeddedDataSpecifications)
                ),
                assetInformation: new AssetInformation(
                    assetKind:        Serializer.DeserializeElement<AssetKind>(aasDB.AssetKind),
                    specificAssetIds: Serializer.DeserializeList<ISpecificAssetId>(aasDB.SpecificAssetIds),
                    globalAssetId:    aasDB.GlobalAssetId,
                    assetType:        aasDB.AssetType,
                    defaultThumbnail: new Resource(
                        path: aasDB.DefaultThumbnailPath,
                        contentType: aasDB.DefaultThumbnailContentType
                    )
                )
            )
            {
                TimeStampCreate = aasDB.TimeStampCreate,
                TimeStamp       = aasDB.TimeStamp,
                TimeStampTree   = aasDB.TimeStampTree,
                TimeStampDelete = aasDB.TimeStampDelete
            };

            return aas;
        }

        public static Submodel? GetSubmodel(SMSet? smDB = null, string smIdentifier = "")
        {
            using (var db = new AasContext())
            {
                if (!smIdentifier.IsNullOrEmpty())
                {
                    var smList = db.SMSets.Where(sm => sm.Identifier == smIdentifier).ToList();
                    if (smList.Count == 0)
                        return null;
                    smDB = smList.First();
                }

                if (smDB == null)
                    return null;

                var SMEList = db.SMESets
                    .OrderBy(sme => sme.Id)
                    .Where(sme => sme.SMId == smDB.Id)
                    .ToList();

                var submodel = new Submodel(
                    idShort:                    smDB.IdShort,
                    displayName:                Serializer.DeserializeList<ILangStringNameType>(smDB.DisplayName),
                    category:                   smDB.Category,
                    description:                Serializer.DeserializeList<ILangStringTextType>(smDB.Description),
                    extensions:                 Serializer.DeserializeList<IExtension>(smDB.Extensions),
                    id:                         smDB.Identifier,
                    kind:                       Serializer.DeserializeElement<ModellingKind>(smDB.Kind),
                    semanticId:                 !smDB.SemanticId.IsNullOrEmpty() ? new Reference(ReferenceTypes.ExternalReference, new List<IKey>() { new Key(KeyTypes.GlobalReference, smDB.SemanticId) }) : null,
                    supplementalSemanticIds:    Serializer.DeserializeList<IReference>(smDB.SupplementalSemanticIds),
                    qualifiers:                 Serializer.DeserializeList<IQualifier>(smDB.Qualifiers),
                    embeddedDataSpecifications: Serializer.DeserializeList<IEmbeddedDataSpecification>(smDB.EmbeddedDataSpecifications),
                    administration: new AdministrativeInformation(
                        version: smDB.Version,
                        revision: smDB.Revision,
                        creator: Serializer.DeserializeElement<IReference>(smDB.Creator),
                        templateId: smDB.TemplateId,
                        embeddedDataSpecifications: Serializer.DeserializeList<IEmbeddedDataSpecification>(smDB.AEmbeddedDataSpecifications)
                    ),
                    submodelElements:           new List<ISubmodelElement>()
                );

                LoadSME(submodel, null, null, SMEList);

                submodel.TimeStampCreate = smDB.TimeStampCreate;
                submodel.TimeStamp       = smDB.TimeStamp;
                submodel.TimeStampTree   = smDB.TimeStampTree;
                submodel.TimeStampDelete = smDB.TimeStampDelete;
                submodel.SetAllParents();

                return submodel;
            }
        }

        private static void LoadSME(Submodel submodel, ISubmodelElement? sme, SMESet? smeSet, List<SMESet> SMEList)
        {
            var smeSets = SMEList.Where(s => s.ParentSMEId == (smeSet != null ? smeSet.Id : null)).OrderBy(s => s.IdShort).ToList();

            foreach (var smel in smeSets)
            {
                // prefix of operation
                var split = !smel.SMEType.IsNullOrEmpty() ? smel.SMEType.Split(VisitorAASX.OPERATION_SPLIT) : [ string.Empty ];
                var oprPrefix = split.Length == 2 ? split[ 0 ] : string.Empty;
                smel.SMEType  = split.Length == 2 ? split[ 1 ] : split[ 0 ];

                // create SME from database
                var nextSME = CreateSME(smel);

                // add sme to sm or sme 
                if (sme == null)
                {
                    submodel.Add(nextSME);
                }
                else
                {
                    switch (smeSet.SMEType)
                    {
                        case "RelA":
                            (sme as AnnotatedRelationshipElement).Annotations.Add((IDataElement) nextSME);
                            break;
                        case "SML":
                            (sme as SubmodelElementList).Value.Add(nextSME);
                            break;
                        case "SMC":
                            (sme as SubmodelElementCollection).Value.Add(nextSME);
                            break;
                        case "Ent":
                            (sme as Entity).Statements.Add(nextSME);
                            break;
                        case "Opr":
                            if (oprPrefix.Equals(VisitorAASX.OPERATION_INPUT))
                                (sme as Operation).InputVariables.Add(new OperationVariable(nextSME));
                            else if (oprPrefix.Equals(VisitorAASX.OPERATION_OUTPUT))
                                (sme as Operation).OutputVariables.Add(new OperationVariable(nextSME));
                            else if (oprPrefix.Equals(VisitorAASX.OPERATION_INOUTPUT))
                                (sme as Operation).InoutputVariables.Add(new OperationVariable(nextSME));
                            break;
                    }
                }

                // recursiv, call for child sme's
                switch (smel.SMEType)
                {
                    case "RelA":
                    case "SML":
                    case "SMC":
                    case "Ent":
                    case "Opr":
                        LoadSME(submodel, nextSME, smel, SMEList);
                        break;
                }
            }
        }

        private static ISubmodelElement? CreateSME(SMESet smeSet)
        {
            ISubmodelElement? sme = null;
            var value = smeSet.GetValue();
            var oValue = smeSet.GetOValue();

            switch (smeSet.SMEType)
            {
                case "Rel":
                    sme = new RelationshipElement(
                        first:  Serializer.DeserializeElement<IReference>(oValue.ContainsKey("First")  ? oValue["First"]  : null, true),
                        second: Serializer.DeserializeElement<IReference>(oValue.ContainsKey("Second") ? oValue["Second"] : null, true));
                    break;
                case "RelA":
                    sme = new AnnotatedRelationshipElement(
                        first:       Serializer.DeserializeElement<IReference>(oValue.ContainsKey("First")  ? oValue["First"]  : null, true),
                        second:      Serializer.DeserializeElement<IReference>(oValue.ContainsKey("Second") ? oValue["Second"] : null, true),
                        annotations: new List<IDataElement>());
                    break;
                case "Prop":
                    sme = new Property(
                        value:     value.First()[0],
                        valueType: Serializer.DeserializeElement<DataTypeDefXsd>(value.First()[1], true),
                        valueId:   Serializer.DeserializeElement<IReference>(oValue.ContainsKey("ValueId") ? oValue["ValueId"] : null));
                    break;
                case "MLP":
                    sme = new MultiLanguageProperty(
                        value:   value.ConvertAll<ILangStringTextType>(val => new LangStringTextType(val[1], val[0])),
                        valueId: Serializer.DeserializeElement<IReference>(oValue.ContainsKey("ValueId") ? oValue["ValueId"] : null));
                    break;
                case "Range":
                    sme = new AasCore.Aas3_0.Range(
                        valueType: Serializer.DeserializeElement<DataTypeDefXsd>(oValue["ValueType"], true),
                        min:       value.Find(val => val[1].Equals("Min")).FirstOrDefault(string.Empty),
                        max:       value.Find(val => val[1].Equals("Max")).FirstOrDefault(string.Empty));
                    break;
                case "Blob":
                    sme = new Blob(
                        value:       Encoding.ASCII.GetBytes(value.First()[0]),
                        contentType: value.First()[1]);
                    break;
                case "File":
                    sme = new AasCore.Aas3_0.File(
                        value:       value.First()[0],
                        contentType: value.First()[1]);
                    break;
                case "Ref":
                    sme = new ReferenceElement(
                        value: Serializer.DeserializeElement<IReference>(oValue.ContainsKey("Value") ? oValue["Value"] : null));
                    break;
                case "Cap":
                    sme = new Capability();
                    break;
                case "SML":
                    sme = new SubmodelElementList(
                        orderRelevant:         Serializer.DeserializeElement<bool>(oValue.ContainsKey("OrderRelevant") ? oValue["OrderRelevant"] : null, true),
                        semanticIdListElement: Serializer.DeserializeElement<IReference>(oValue.ContainsKey("SemanticIdListElement") ? oValue["SemanticIdListElement"] : null),
                        typeValueListElement:  Serializer.DeserializeElement<AasSubmodelElements>(oValue.ContainsKey("TypeValueListElement") ? oValue["TypeValueListElement"] : null, true),
                        valueTypeListElement:  Serializer.DeserializeElement<DataTypeDefXsd>(oValue.ContainsKey("ValueTypeListElement") ? oValue["ValueTypeListElement"] : null),
                        value:                 new List<ISubmodelElement>());
                    break;
                case "SMC":
                    sme = new SubmodelElementCollection(
                        value: new List<ISubmodelElement>());
                    break;
                case "Ent":
                    sme = new Entity(
                        statements:       new List<ISubmodelElement>(),
                        entityType:       Serializer.DeserializeElement<EntityType>(value.First()[1], true),
                        globalAssetId:    value.First()[0],
                        specificAssetIds: Serializer.DeserializeList<ISpecificAssetId>(oValue.ContainsKey("SpecificAssetIds") ? oValue["SpecificAssetIds"] : null));
                    break;
                case "Evt":
                    sme = new BasicEventElement(
                        observed:      Serializer.DeserializeElement<IReference>(oValue.ContainsKey("Observed") ? oValue["Observed"] : null),
                        direction:     Serializer.DeserializeElement<Direction>(oValue.ContainsKey("Direction") ? oValue["Direction"] : null, true),
                        state:         Serializer.DeserializeElement<StateOfEvent>(oValue.ContainsKey("State") ? oValue["State"] : null, true),
                        messageTopic:  oValue.ContainsKey("MessageTopic") ? oValue["MessageTopic"] : null,
                        messageBroker: Serializer.DeserializeElement<IReference>(oValue.ContainsKey("MessageBroker") ? oValue["MessageBroker"] : null),
                        lastUpdate:    oValue.ContainsKey("LastUpdate") ? oValue["LastUpdate"] : null,
                        minInterval:   oValue.ContainsKey("MinInterval") ? oValue["MinInterval"] : null,
                        maxInterval:   oValue.ContainsKey("MaxInterval") ? oValue["MaxInterval"] : null);
                    break;
                case "Opr":
                    sme = new Operation(
                        inputVariables:    new List<IOperationVariable>(),
                        outputVariables:   new List<IOperationVariable>(),
                        inoutputVariables: new List<IOperationVariable>());
                    break;
            }

            if (sme == null)
                return null;

            sme.IdShort                    = smeSet.IdShort;
            sme.DisplayName                = Serializer.DeserializeList<ILangStringNameType>(smeSet.DisplayName);
            sme.Category                   = smeSet.Category;
            sme.Description                = Serializer.DeserializeList<ILangStringTextType>(smeSet.Description);
            sme.Extensions                 = Serializer.DeserializeList<IExtension>(smeSet.Extensions);
            sme.SemanticId                 = !smeSet.SemanticId.IsNullOrEmpty() ? new Reference(ReferenceTypes.ExternalReference, new List<IKey>() { new Key(KeyTypes.GlobalReference, smeSet.SemanticId) }) : null;
            sme.SupplementalSemanticIds    = Serializer.DeserializeList<IReference>(smeSet.SupplementalSemanticIds);
            sme.Qualifiers                 = Serializer.DeserializeList<IQualifier>(smeSet.Qualifiers);
            sme.EmbeddedDataSpecifications = Serializer.DeserializeList<IEmbeddedDataSpecification>(smeSet.EmbeddedDataSpecifications);
            sme.TimeStampCreate            = smeSet.TimeStampCreate;
            sme.TimeStamp                  = smeSet.TimeStamp;
            sme.TimeStampTree              = smeSet.TimeStampTree;
            sme.TimeStampDelete            = smeSet.TimeStampDelete;
            return sme;
        }

        public static string GetAASXPath(int? envId = null, string cdId = "", string aasId = "", string smId = "")
        {
            using var db = new AasContext();
            if (!cdId.IsNullOrEmpty())
            {
                var cdDBList = db.CDSets.Where(cd => cd.Identifier == cdId).Join(db.EnvCDSets, cd => cd.Id, envcd => envcd.CDId, (cd, envcd) => envcd);
                if (cdDBList.Any())
                    envId = cdDBList.First().EnvId;
            }

            if (!smId.IsNullOrEmpty())
            {
                var smDBList = db.SMSets.Where(s => s.Identifier == smId);
                if (smDBList.Any())
                    envId = smDBList.First().EnvId;
            }

            if (!aasId.IsNullOrEmpty())
            {
                var aasDBList = db.AASSets.Where(a => a.Identifier == aasId);
                if (aasDBList.Any())
                    envId = aasDBList.First().EnvId;
            }

            if (envId == null)
                return string.Empty;

            var path = db.EnvSets.Where(e => e.Id == envId).Select(e => e.Path).FirstOrDefault();
            return path ?? string.Empty;
        }
    }
}