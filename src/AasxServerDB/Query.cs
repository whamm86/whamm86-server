using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AasxServerDB.Entities;
using AasxServerDB.Result;
using Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using System.Linq.Dynamic.Core;
using QueryParserTest;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.X86;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace AasxServerDB
{
    public class Query
    {
        public static string? ExternalBlazor { get; set; }

        // --------------- Help Class ---------------
        private class CombinedSMResult
        {
            public string? Identifier;
            public DateTime TimeStampTree;
        }

        public class CombinedSMEResult
        {
            // public int? sm_Id { get; set; } = null;
            public string? Identifier { get; set; } = null;
            public int? ParentSMEId { get; set; } = null;
            public string? IdShort { get; set; } = null;
            public DateTime? TimeStamp { get; set; } = null;
            public string? Value { get; set; } = null;
        }

        // --------------- API ---------------
        public List<SMResult> SearchSMs(string semanticId = "", string identifier = "", string diff = "", string expression = "")
        {
            var watch = Stopwatch.StartNew();
            using AasContext db = new();
            Console.WriteLine("\nSearchSMs");

            watch.Restart();
            var query = GetSMs(db, semanticId, identifier, diff, expression);
            Console.WriteLine("Generate query\tin " + watch.ElapsedMilliseconds + " ms");

            watch.Restart();
            var result = GetSMResult(query);
            Console.WriteLine("Collect results\tin " + watch.ElapsedMilliseconds + " ms\nSMs found\t" + result.Count + "/" + db.SMSets.Count());

            return result;
        }

        public int CountSMs(string semanticId = "", string identifier = "", string diff = "", string expression = "")
        {
            var watch = Stopwatch.StartNew();
            using AasContext db = new();
            Console.WriteLine("\nCountSMs");

            watch.Restart();
            var query = GetSMs(db, semanticId, identifier, diff, expression);
            Console.WriteLine("Generate query\tin " + watch.ElapsedMilliseconds + " ms");

            watch.Restart();
            var result = query.Count();
            Console.WriteLine("Collect results\tin " + watch.ElapsedMilliseconds + " ms\nSMs found\t" + result + "/" + db.SMSets.Count());

            return result;
        }

        public List<SMEResult> SearchSMEs(
            string smSemanticId = "", string smIdentifier = "", string semanticId = "", string diff = "",
            string contains = "", string equal = "", string lower = "", string upper = "", string expression = "")
        {
            var watch = Stopwatch.StartNew();
            using AasContext db = new();
            Console.WriteLine("\nSearchSMEs");

            watch.Restart();
            var query = GetSMEs(db, smSemanticId, smIdentifier, semanticId, diff, contains, equal, lower, upper, expression);
            Console.WriteLine("Generate query\tin " + watch.ElapsedMilliseconds + " ms");

            watch.Restart();
            var result = GetSMEResult(db, query);
            Console.WriteLine("Collect results\tin " + watch.ElapsedMilliseconds + " ms\nSMEs found\t" + result.Count + "/" + db.SMESets.Count());

            return result;
        }

        public int CountSMEs(
            string smSemanticId = "", string smIdentifier = "", string semanticId = "", string diff = "",
            string contains = "", string equal = "", string lower = "", string upper = "", string expression = "")
        {
            var watch = Stopwatch.StartNew();
            using AasContext db = new();
            Console.WriteLine("\nCountSMEs");

            watch.Restart();
            var query = GetSMEs(db, smSemanticId, smIdentifier, semanticId, diff, contains, equal, lower, upper, expression);
            Console.WriteLine("Generate query\tin " + watch.ElapsedMilliseconds + " ms");

            watch.Restart();
            var result = query.Count();
            Console.WriteLine("Collect results\tin " + watch.ElapsedMilliseconds + " ms\nSMEs found\t" + result + "/" + db.SMESets.Count());

            return result;
        }

        public List<SMEResult> SearchSMEsResult(
            string smSemanticId = "",
            string searchSemanticId = "",  string searchIdShort = "",
            string? equal = "", string? contains = "",
            string resultSemanticId = "", string resultIdShort = "")
        {
            List<SMEResult> result = new List<SMEResult>();

            if (searchSemanticId.IsNullOrEmpty() && searchIdShort.IsNullOrEmpty())
                return result;
            if (equal.IsNullOrEmpty() && contains.IsNullOrEmpty())
                return result;
            if (resultSemanticId.IsNullOrEmpty() && resultIdShort.IsNullOrEmpty())
                return result;

            bool withI = false;
            long iEqual = 0;
            bool withF = false;
            double fEqual = 0;
            try
            {
                if (!equal.IsNullOrEmpty())
                {
                    iEqual = Convert.ToInt64(equal);
                    withI = true;
                    fEqual = Convert.ToDouble(equal);
                    withF = true;
                }
            }
            catch { }

            using (AasContext db = new AasContext())
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("SearchSMEs");
                Console.WriteLine("Total number of SMEs " + db.SMESets.Count() + " in " + watch.ElapsedMilliseconds + "ms");
                watch.Restart();

                bool withContains = (!contains.IsNullOrEmpty());
                bool withEqual = !withContains && (!equal.IsNullOrEmpty());

                var list = db.SValueSets.Where(v =>
                    (withContains && v.Value.Contains(contains)) ||
                    (withEqual && v.Value == equal)
                    )
                    .Join(db.SMESets,
                        v => v.SMEId,
                        sme => sme.Id,
                        (v, sme) => new
                        {
                            SMId = sme.SMId,
                            SemanticId = sme.SemanticId,
                            IdShort = sme.IdShort,
                            ParentSme = sme.ParentSMEId,
                            Value = v.Value
                        }
                    )
                    .Where(s =>
                        (!searchSemanticId.IsNullOrEmpty() && s.SemanticId == searchSemanticId) ||
                        (!searchIdShort.IsNullOrEmpty() && s.IdShort == searchIdShort)
                    )
                    .Join(db.SMSets,
                        v => v.SMId,
                        s => s.Id,
                        (v, s) => new
                        {
                            Id = s.Id,
                            SemanticId = s.SemanticId,
                            ParentSme = v.ParentSme,
                            Value = v.Value
                        }
                    )
                    .Where(s =>
                        smSemanticId == "" || s.SemanticId == smSemanticId
                    )
                    .ToList();

                list.AddRange(db.IValueSets.Where(v =>
                    (withEqual && withI && v.Value == iEqual)
                    )
                    .Join(db.SMESets,
                        v => v.SMEId,
                        sme => sme.Id,
                        (v, sme) => new
                        {
                            SMId = sme.SMId,
                            SemanticId = sme.SemanticId,
                            IdShort = sme.IdShort,
                            ParentSme = sme.ParentSMEId,
                            Value = v.Value.ToString()
                        }
                    )
                    .Where(s =>
                        (!searchSemanticId.IsNullOrEmpty() && s.SemanticId == searchSemanticId) ||
                        (!searchIdShort.IsNullOrEmpty() && s.IdShort == searchIdShort)
                    )
                    .Join(db.SMSets,
                        v => v.SMId,
                        s => s.Id,
                        (v, s) => new
                        {
                            Id = s.Id,
                            SemanticId = s.SemanticId,
                            ParentSme = v.ParentSme,
                            Value = v.Value
                        }
                    )
                    .Where(s =>
                        smSemanticId == "" || s.SemanticId == smSemanticId
                    )
                    .ToList());

                list.AddRange(db.DValueSets.Where(v =>
                    (withEqual && withF && v.Value == fEqual)
                    )
                    .Join(db.SMESets,
                        v => v.SMEId,
                        sme => sme.Id,
                        (v, sme) => new
                        {
                            SMId = sme.SMId,
                            SemanticId = sme.SemanticId,
                            IdShort = sme.IdShort,
                            ParentSme = sme.ParentSMEId,
                            Value = v.Value.ToString()
                        }
                    )
                    .Where(s =>
                        (!searchSemanticId.IsNullOrEmpty() && s.SemanticId == searchSemanticId) ||
                        (!searchIdShort.IsNullOrEmpty() && s.IdShort == searchIdShort)
                    )
                    .Join(db.SMSets,
                        v => v.SMId,
                        s => s.Id,
                        (v, s) => new
                        {
                            Id = s.Id,
                            SemanticId = s.SemanticId,
                            ParentSme = v.ParentSme,
                            Value = v.Value
                        }
                    )
                    .Where(s =>
                        smSemanticId == "" || s.SemanticId == smSemanticId
                    )
                    .ToList());

                Console.WriteLine("Found " + list.Count() + " SMEs in " + watch.ElapsedMilliseconds + "ms");

                var hSubmodel = new HashSet<long>();
                var lParentParentNum = new List<int?>();
                foreach (var l in list)
                {
                    hSubmodel.Add(l.Id);
                    var smeDB = db.SMESets.Where(s => s.Id == l.ParentSme).First();
                    lParentParentNum.Add(smeDB.ParentSMEId);
                }

                Console.WriteLine("Found " + hSubmodel.Count() + " Submodels");

                watch.Restart();

                var SMEResult = db.SMESets.Where(s =>
                    hSubmodel.Contains(s.SMId) &&
                    ((!resultSemanticId.IsNullOrEmpty() && s.SemanticId == resultSemanticId) ||
                    (!resultIdShort.IsNullOrEmpty() && s.IdShort == resultIdShort))
                    )
                    .ToList();

                if (equal.IsNullOrEmpty())
                    equal = contains;

                foreach (var l in SMEResult)
                {
                    SMEResult r = new SMEResult();
                    bool found = false;

                    var submodelDB = db.SMSets.Where(s => s.Id == l.SMId).First();
                    if (submodelDB != null && (smSemanticId.IsNullOrEmpty() || submodelDB.SemanticId == smSemanticId))
                    {
                        r.value = equal;
                        r.url = string.Empty;
                        r.smId = submodelDB.Identifier;
                        string path = l.IdShort;
                        int? pId = l.ParentSMEId;
                        while (pId != null)
                        {
                            var smeDB = db.SMESets.Where(s => s.Id == pId).First();
                            path = smeDB.IdShort + "." + path;
                            pId = smeDB.ParentSMEId;
                            if (lParentParentNum.Contains(pId))
                            {
                                found = true;
                                if (l.SMEType == "D")
                                {
                                    var v = db.SValueSets.Where(v => v.SMEId == l.Id).FirstOrDefault();
                                    if (v.Value.ToLower().StartsWith("http"))
                                        r.url = v.Value;
                                }
                            }
                        }
                        r.idShortPath = path;
                        string sub64 = Base64UrlEncoder.Encode(r.smId);
                        if (r.url.IsNullOrEmpty())
                            r.url = ExternalBlazor + "/submodels/" + sub64 + "/submodel-elements/" + path + "/attachment";
                        if (found)
                            result.Add(r);
                    }
                }
                Console.WriteLine("Collected result in " + watch.ElapsedMilliseconds + "ms");
            }

            return result;
        }

        // --------------- SM Methodes ---------------
        private static IQueryable<CombinedSMResult> GetSMs(AasContext db, string semanticId = "", string identifier = "", string diffString = "", string expression = "")
        {
            // analyse parameters
            var withSemanticId = !semanticId.IsNullOrEmpty();
            var withIdentifier = !identifier.IsNullOrEmpty();
            var diff = TimeStamp.TimeStamp.StringToDateTime(diffString);
            var withDiff = !diff.Equals(DateTime.MinValue);
            var withParameters = withSemanticId || withIdentifier || withDiff;
            var withExpression = !expression.IsNullOrEmpty();

            // get data
            IQueryable<SMSet> result;
            if (withExpression && !withParameters)
            {
                // shorten expression
                expression = expression.Replace("$REVERSE", "").Replace("$LOG", "");
                expression = Regex.Replace(expression, @"\s+", string.Empty);

                // init parser
                var countTypePrefix = 0;
                var parser = new ParserWithAST(new Lexer(expression));
                var ast = parser.Parse();

                // combined condition
                var combinedCondition = parser.GenerateSql(ast, "", ref countTypePrefix, "filter");

                // sm condition
                var conditionSM = parser.GenerateSql(ast, "", ref countTypePrefix, "filter_submodel");
                if (conditionSM.IsNullOrEmpty())
                    conditionSM = parser.GenerateSql(ast, "sm.", ref countTypePrefix, "filter");
                conditionSM = conditionSM.Replace("sm.", "");

                // check restrictions
                var restrictSM = !conditionSM.IsNullOrEmpty() && !conditionSM.Equals("true");

                // get data
                if (restrictSM)
                    result = db.SMSets.Where(conditionSM);
                else
                    result = Enumerable.Empty<SMSet>().AsQueryable();
            }
            else if (!withExpression && withParameters)
            {
                result = db.SMSets
                    .Where(s =>
                        (!withSemanticId || (s.SemanticId != null && s.SemanticId.Equals(semanticId))) &&
                        (!withIdentifier || (s.Identifier != null && s.Identifier.Equals(identifier))) &&
                        (!withDiff || s.TimeStampTree.CompareTo(diff) > 0));
            }
            else
                result = Enumerable.Empty<SMSet>().AsQueryable();
            return result.Select(sm => new CombinedSMResult { Identifier = sm.Identifier, TimeStampTree = sm.TimeStampTree });
        }

        private static List<SMResult> GetSMResult(IQueryable<CombinedSMResult> query)
        {
            var result = new List<SMResult>();
            foreach (var sm in query)
            {
                result.Add(
                    new SMResult{
                        smId = sm.Identifier ?? "",
                        url = $"{ExternalBlazor}/submodels/{Base64UrlEncoder.Encode(sm.Identifier ?? "")}",
                        timeStampTree = TimeStamp.TimeStamp.DateTimeToString(sm.TimeStampTree)
                    }
                );
            }
            return result;
        }

        // --------------- SME Methodes ---------------
        private IQueryable<CombinedSMEResult> GetSMEs(AasContext db, string smSemanticId = "", string smIdentifier = "", string semanticId = "",
            string diffString = "", string contains = "", string equal = "", string lower = "", string upper = "", string expression = "")
        {
            // analyse parameters
            var withSMSemanticId = !smSemanticId.IsNullOrEmpty();
            var withSMIdentifier = !smIdentifier.IsNullOrEmpty();
            var withSemanticId = !semanticId.IsNullOrEmpty();
            var diff = TimeStamp.TimeStamp.StringToDateTime(diffString);
            var withDiff = !diff.Equals(DateTime.MinValue);
            var withContains = !contains.IsNullOrEmpty();
            var withEqualString = !equal.IsNullOrEmpty();
            var withEqualNum = Int64.TryParse(equal, out long equalNum);
            var withLower = Int64.TryParse(lower, out long lowerNum);
            var withUpper = Int64.TryParse(upper, out long upperNum);
            var withCompare = withLower && withUpper;
            var withParameters = withSMSemanticId || withSMIdentifier || withSemanticId || withDiff || withContains || withEqualString || withLower || withUpper;
            var withExpression = !expression.IsNullOrEmpty();

            // direction
            var direction = 0; // 0 = top-down, 1 = middle-out, 2 down-top
            var restrictValue = false;

            // implementierung OValueSet

            // restrict all tables seperate
            IQueryable<SMSet> smTabel;
            IQueryable<SMESet> smeTabel;
            IQueryable<SValueSet>? sValueTabel;
            IQueryable<IValueSet>? iValueTabel;
            IQueryable<DValueSet>? dValueTabel;

            // get data
            if (withExpression && !withParameters)
            {
                // direction
                direction = !expression.Contains("$REVERSE") ? 0 : 2;

                // shorten expression
                expression = expression.Replace("$REVERSE", "").Replace("$LOG", "");
                expression = Regex.Replace(expression, @"\s+", string.Empty);

                // init parser
                var countTypePrefix = 0;
                var parser = new ParserWithAST(new Lexer(expression));
                var ast = parser.Parse();

                // combined condition
                var combinedCondition = parser.GenerateSql(ast, "", ref countTypePrefix, "filter");

                // sm condition
                var conditionSM = parser.GenerateSql(ast, "", ref countTypePrefix, "filter_submodel");
                if (conditionSM.IsNullOrEmpty())
                    conditionSM = parser.GenerateSql(ast, "sm.", ref countTypePrefix, "filter");
                conditionSM = conditionSM.Replace("sm.", "");

                // sme condition
                var conditionSME = parser.GenerateSql(ast, "", ref countTypePrefix, "filter_submodel_elements");
                if (conditionSME.IsNullOrEmpty())
                    conditionSME = parser.GenerateSql(ast, "sme.", ref countTypePrefix, "filter");
                conditionSME = conditionSME.Replace("sme.", "");

                // value condition
                var conditionSMEValue = parser.GenerateSql(ast, "sme.value", ref countTypePrefix, "filter");

                // string condition
                var conditionStr = parser.GenerateSql(ast, "", ref countTypePrefix, "filter_str");
                if (conditionStr.IsNullOrEmpty())
                    conditionStr = parser.GenerateSql(ast, "sValue", ref countTypePrefix, "filter");
                if (conditionStr.Equals("true"))
                    conditionStr = "sValue != null";
                conditionStr = conditionStr.Replace("sValue", "Value");

                // num condition
                var conditionNum = parser.GenerateSql(ast, "", ref countTypePrefix, "filter_num");
                if (conditionNum.IsNullOrEmpty())
                    conditionNum = parser.GenerateSql(ast, "mValue", ref countTypePrefix, "filter");
                if (conditionNum.Equals("true"))
                    conditionNum = "mValue != null";
                conditionNum = conditionNum.Replace("mValue", "Value");

                // check restrictions
                var restrictSM = !conditionSM.IsNullOrEmpty() && !conditionSM.Equals("true");
                var restrictSME = !conditionSME.IsNullOrEmpty() && !conditionSME.Equals("true");
                var restrictSVaue = !conditionStr.IsNullOrEmpty() && !conditionStr.Equals("true");
                var restrictNumVaue = !conditionNum.IsNullOrEmpty() && !conditionNum.Equals("true");
                restrictValue = restrictSVaue || restrictNumVaue;

                // restrict all tables seperate 
                smTabel = restrictSM ? db.SMSets.Where(conditionSM) : db.SMSets;
                smeTabel = restrictSME ? db.SMESets.Where(conditionSME) : db.SMESets;
                sValueTabel = restrictValue ? (restrictSVaue ? db.SValueSets.Where(conditionStr) : null) : db.SValueSets;
                iValueTabel = restrictValue ? (restrictNumVaue ? db.IValueSets.Where(conditionStr) : null) : db.IValueSets;
                dValueTabel = restrictValue ? (restrictNumVaue ? db.DValueSets.Where(conditionStr) : null) : db.DValueSets;
            }
            else if (!withExpression && withParameters)
            {
                // direction
                direction = (withSMSemanticId || withSMIdentifier) ? 0 : ((withContains || withEqualString || withLower || withUpper) ? 2 : 1);

                // check restrictions
                var restrictSM = withSMSemanticId || withSMIdentifier;
                var restrictSME = withSemanticId || withDiff;
                var restrictSValue = withContains || withEqualString;
                var restrictNumValue = withEqualNum || withCompare;
                restrictValue = restrictSValue || restrictNumValue;
                if ((withEqualString && (withContains || withLower || withUpper)) || (withContains && (withLower || withUpper)) || (withLower != withUpper))
                    return Enumerable.Empty<CombinedSMEResult>().AsQueryable();

                // restrict all tables seperate 
                smTabel = restrictSM ? db.SMSets.Where(sm => (!withSMSemanticId || (sm.SemanticId != null && sm.SemanticId.Equals(smSemanticId))) && (!withSMIdentifier || (sm.Identifier != null && sm.Identifier.Equals(smIdentifier)))) : db.SMSets;
                smeTabel = restrictSME ? db.SMESets.Where(sme => (!withSemanticId || (sme.SemanticId != null && sme.SemanticId.Equals(semanticId))) && (!withDiff || sme.TimeStamp.CompareTo(diff) > 0)) : db.SMESets;
                sValueTabel = restrictValue ? (restrictSValue   ? db.SValueSets.Where(v => restrictSValue   && v.Value != null && (!withContains || v.Value.Contains(contains)) && (!withEqualString || v.Value.Equals(equal))) : null) : db.SValueSets;
                iValueTabel = restrictValue ? (restrictNumValue ? db.IValueSets.Where(v => restrictNumValue && v.Value != null && (!withEqualNum || v.Value == equalNum) && (!withCompare || (v.Value >= lowerNum && v.Value <= upperNum))) : null) : db.IValueSets;
                dValueTabel = restrictValue ? (restrictNumValue ? db.DValueSets.Where(v => restrictNumValue && v.Value != null && (!withEqualNum || v.Value == equalNum) && (!withCompare || (v.Value >= lowerNum && v.Value <= upperNum))) : null) : db.DValueSets;
            }
            else
                return Enumerable.Empty<CombinedSMEResult>().AsQueryable();

            // select
            var smSelect = smTabel.Select(sm => new { sm.Id, sm.Identifier });
            var smeSelect = smeTabel.Select(sme => new { sme.SMId, sme.Id, sme.ParentSMEId, sme.IdShort, sme.TimeStamp, sme.TValue });
            var sValueSelect = sValueTabel?.Select(sV => new { sV.SMEId, sV.Value });
            var iValueSelect = iValueTabel?.Select(iV => new { iV.SMEId, iV.Value });
            var dValueSelect = dValueTabel?.Select(dV => new { dV.SMEId, dV.Value });

            // to query string
            var smQueryString = smSelect.ToQueryString();
            var smeQueryString = smeSelect.ToQueryString();
            var sValueQueryString = sValueSelect?.ToQueryString();
            var iValueQueryString = iValueSelect?.ToQueryString();
            var dValueQueryString = dValueSelect?.ToQueryString();

            // set parameters
            smQueryString = SetParameter(smQueryString);
            smeQueryString = SetParameter(smeQueryString);
            sValueQueryString = sValueQueryString != null ? SetParameter(sValueQueryString) : null;
            iValueQueryString = iValueQueryString != null ? SetParameter(iValueQueryString) : null;
            dValueQueryString = dValueQueryString != null ? SetParameter(dValueQueryString) : null;

            // change INSTR to LIKE
            sValueQueryString = sValueQueryString != null && withContains ? ChangeINSTRToLIKE(sValueQueryString) : null;

            // with querys
            var selectWithStart = $"SELECT sme.SMId, sme.ParentSMEId, sme.IdShort, sme.TimeStamp, v.Value \r\n" +
                $"FROM";
            var selectWithEnd = $"AS v \r\n" +
                ((withContains || withEqualString || withCompare) ? "INNER" : "RIGHT") +
                $" JOIN FilteredSME AS sme \r\n" +
                $"ON sme.Id = v.SMEId \r\n ";
            var withQueryString = $"WITH\r\n" +
                $"FilteredSM AS (\r\n{smQueryString}\r\n),\r\n" +
                $"FilteredSME AS (\r\n{smeQueryString}\r\n),\r\n" +
                (sValueQueryString != null ? $"FilteredSValue AS (\r\n{sValueQueryString}\r\n),\r\n" : string.Empty) +
                (iValueQueryString != null ? $"FilteredIValue AS (\r\n{iValueQueryString}\r\n),\r\n" : string.Empty) +
                (dValueQueryString != null ? $"FilteredDValue AS (\r\n{dValueQueryString}\r\n),\r\n" : string.Empty) +
                (direction == 0 ?
                    $"FilteredSMAndSME AS ( \r\n" +
                        $"SELECT sme.SMId, sme.Id, sm.Identifier, sme.ParentSMEId, sme.IdShort, sme.TimeStamp \r\n" +
                        $"FROM FilteredSM AS sm \r\n" +
                        $"INNER JOIN FilteredSME AS sme \r\n" +
                        $"ON sm.Id = sme.SMId \r\n" +
                    $")" : string.Empty) +
                (direction == 1 ?
                    $"FilteredSMAndSME AS ( \r\n" +
                        $"SELECT sme.SMId, sme.Id, sm.Identifier, sme.ParentSMEId, sme.IdShort, sme.TimeStamp \r\n" +
                        $"FROM FilteredSME AS sme \r\n" +
                        $"INNER JOIN FilteredSM AS sm \r\n" +
                        $"ON sm.Id = sme.SMId \r\n" +
                    $")" : string.Empty) +
                (direction == 2 ?
                    $"FilteredSMEAndValue AS (\r\n" +
                        (!sValueQueryString.IsNullOrEmpty() ? $"{selectWithStart} FilteredSValue {selectWithEnd}" : string.Empty) +
                        ((!sValueQueryString.IsNullOrEmpty() && !iValueQueryString.IsNullOrEmpty()) ? "UNION ALL \r\n" : string.Empty) +
                        (!iValueQueryString.IsNullOrEmpty() ? $"{selectWithStart} FilteredIValue {selectWithEnd}" : string.Empty) +
                        ((!iValueQueryString.IsNullOrEmpty() && !dValueQueryString.IsNullOrEmpty()) ? "UNION ALL \r\n" : string.Empty) +
                        (!dValueQueryString.IsNullOrEmpty() ? $"{selectWithStart} FilteredDValue {selectWithEnd}" : string.Empty) +
                    $")" : string.Empty);

            // join the restricted tables
            var selectStart = $"SELECT sm_sme.Identifier, sm_sme.ParentSMEId, sm_sme.IdShort, sm_sme.TimeStamp, v.Value \r\n" +
                $"FROM FilteredSMAndSME AS sm_sme \r\n" +
                ((withContains || withEqualString || withCompare) ? "INNER" : "LEFT") +
                $" JOIN";
            var selectEnd = $"AS v \r\n" +
                $"ON sm_sme.Id = v.SMEId \r\n ";
            var select =
                (direction == 0 || direction == 1) ? // top-down and middle-out
                    (!sValueQueryString.IsNullOrEmpty() ? $"{selectStart} FilteredSValue {selectEnd}" : string.Empty) +
                    ((!sValueQueryString.IsNullOrEmpty() && !iValueQueryString.IsNullOrEmpty()) ? "UNION ALL \r\n" : string.Empty) +
                    (!iValueQueryString.IsNullOrEmpty() ? $"{selectStart} FilteredIValue {selectEnd}" : string.Empty) +
                    ((!iValueQueryString.IsNullOrEmpty() && !dValueQueryString.IsNullOrEmpty()) ? "UNION ALL \r\n" : string.Empty) +
                    (!dValueQueryString.IsNullOrEmpty() ? $"{selectStart} FilteredDValue {selectEnd}" : string.Empty)
                : // down-top
                    $"SELECT sm.Identifier, sme_v.ParentSMEId, sme_v.IdShort, sme_v.TimeStamp, sme_v.Value \r\n" +
                    $"FROM FilteredSMEAndValue AS sme_v \r\n" +
                    $"INNER JOIN FilteredSM AS sm \r\n" +
                    $"ON sme_v.SMId = sm.Id";

            // create queryable
            var rawSql = $"{withQueryString}\r\n{select}";
            return db.Database.SqlQueryRaw<CombinedSMEResult>(rawSql);
        }

        private string SetParameter(string rawSQL)
        {
            var splitParam = rawSQL.Split(new string[] { ".param set ", "\r\n\r\n" }, StringSplitOptions.None);
            var result = splitParam[splitParam.Length - 1];
            for (var i = 1; i < splitParam.Length - 1; i++)
            {
                var param = splitParam[i].Split(new[] { ' ' }, 2);
                result = result.Replace(param[0], param[1]);
            }
            return result;
        }

        private string ChangeINSTRToLIKE(string rawSQL)
        {
            var splitParam = rawSQL.Split(new string[] { "instr(", ") > 0" }, StringSplitOptions.None);
            var result = splitParam[0];
            for (var i = 1; i < splitParam.Length - 1; i++)
            {
                var param = splitParam[i].Split(", ", 2);
                var paramS = param[1].Split("\'", 3);
                i++;
                result += $"{param[0]} LIKE \'%{paramS[1]}%\' {splitParam[i]}";
            }
            return result;
        }

        private static List<SMEResult> GetSMEResult(AasContext db, IQueryable<CombinedSMEResult> query)
        {
            var result = new List<SMEResult>();
            foreach (var sm_sme_v in query)
            {
                var path = sm_sme_v.IdShort;
                var pId = sm_sme_v.ParentSMEId;
                while (pId != null)
                {
                    var smeDB = db.SMESets.Where(s => s.Id == pId).Select(sme => new { sme.IdShort, sme.ParentSMEId }).First();
                    path = $"{smeDB.IdShort}.{path}";
                    pId = smeDB.ParentSMEId;
                }

                result.Add(new SMEResult()
                    {
                        smId = sm_sme_v.Identifier ?? "",
                        value = sm_sme_v.Value,
                        idShortPath = path,
                        url = $"{ExternalBlazor}/submodels/{Base64UrlEncoder.Encode(sm_sme_v.Identifier ?? "")}/submodel-elements/{path}",
                        timeStamp = TimeStamp.TimeStamp.DateTimeToString((DateTime)sm_sme_v.TimeStamp)
                    }
                );
            }
            return result;
        }
    }
}