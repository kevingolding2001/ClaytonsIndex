using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;
using Dapper;
using System.IO;
using ClaytonsWeb2;

public class Repository {
    private DBConn db;

    public Repository() {
        db = new DBConn();
    }

    public IEnumerable<SearchResult> multiSearch(string search_terms) {
        var multiResult = new List<SearchResult>();
        foreach (var phrase in search_terms.Split('|')) {
            //multiResult.AddRange(search(phrase));
            multiResult.AddRange(FuzzySearch(phrase));
        }

        var result = multiResult.OrderBy(a => a.issue_num).ThenBy(d => d.sub_issue).ThenBy(b => b.pagenum).ThenBy(c => c.linenum)
            .GroupBy(g => new { g.filename, g.issue_num, g.sub_issue, g.pagenum, g.basename, g.description})
            .Select(s => new SearchResult() 
                {
                issue_num = s.Key.issue_num, 
                sub_issue = s.Key.sub_issue, 
                pagenum = s.Key.pagenum,
                filename = s.Key.filename,
                basename = s.Key.basename,
                description = s.Key.description,
                context = string.Join("... ", s.Select(c => c.context))
            });
        return result;
    }

    private List<SearchResult> FuzzySearch(string search_term) {
        var allResult = new List<SearchResult>();

        if (search_term.Length == 0)
        {
            return allResult;
        }

        var words = search_term.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();

        using (var conn = db.GetConn()) {
            conn.Open();
            var sql = @"create temporary table search_result as (select word_index.*, description 
                            from word_index join file_issues using (filename)
                            where word = @word)";
            var comm = conn.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.Add(new MySqlParameter {ParameterName = "@word", Value = words[0]});
            comm.ExecuteNonQuery();

            var idx = 0;
            foreach (var nextWord in words.Skip(1)) {
                idx += 1;
                var join_sql = @"create temporary table join_result as 
                            (select search_result.* from search_result
                                join word_index using (filename)
                                where word_index.wordnum between search_result.wordnum - 5 and search_result.wordnum + 5
                                    and word_index.word = @word )";
                var join_comm = conn.CreateCommand();
                join_comm.CommandText = join_sql;
                join_comm.Parameters.Add(new MySqlParameter {ParameterName = "@word", Value = nextWord});
                join_comm.Parameters.Add(new MySqlParameter {ParameterName = "@idx", Value = idx});
                join_comm.ExecuteNonQuery();

                var inner_count_sql = "select count(*) from join_result";
                var inner_count_comm = conn.CreateCommand();
                inner_count_comm.CommandText = inner_count_sql;
                var rcount = inner_count_comm.ExecuteReader();
                rcount.Read();
                rcount.Close();

                var drop_sql = "drop table search_result";
                var drop_comm = conn.CreateCommand();
                drop_comm.CommandText = drop_sql;
                drop_comm.ExecuteNonQuery();


                var rename_sql = "rename table join_result to search_result";
                var rename_comm = conn.CreateCommand();
                rename_comm.CommandText = rename_sql;
                rename_comm.ExecuteNonQuery();
            }

            var result_sql = @"select search_result.*, issue_num, sub_issue
                                from search_result join file_issues using (filename)
                                order by issue_num, sub_issue";
            var result_comm = conn.CreateCommand();
            result_comm.CommandText = result_sql;

            var reader = result_comm.ExecuteReader();
            while (reader.Read()) {

                var result = new SearchResult();

                //TODO: Need to call build_context here
                //orig: result.context = reader[2].ToString();
                result.context = buildContext(reader[0].ToString(), (int)reader[1], words);
                
                result.filename = reader[0].ToString();
                result.basename = Path.GetFileNameWithoutExtension(result.filename);
                result.issue_num = (int)reader[6];
                result.sub_issue = (string)reader[7];
                result.wordnum = (int)reader[1];
                result.pagenum = (int)reader[3];
                result.linenum = (int)reader[4];
                result.description = (string)reader[5];
                
                allResult.Add(result);
            }
        }

        return allResult;
    }


    public string buildContext(string fileName, int wordNum, string[] searchWords) {
        //TODO: Dapperize this
        var context = "";
        using (var conn = db.GetConn()) {
            conn.Open();
            var sql = @"select word from word_index where filename = @fileName 
                        and wordnum between @wordNum - 4 and @wordNum + 6";
            var comm = conn.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.Add(new MySqlParameter {ParameterName = "fileName", Value = fileName});
            comm.Parameters.Add(new MySqlParameter {ParameterName = "wordNum", Value = wordNum});

            var reader = comm.ExecuteReader();
            while (reader.Read()) {
                var addWord = reader[0].ToString();
                if (searchWords.Contains(addWord, StringComparer.OrdinalIgnoreCase))
                {
                    addWord = $"<span class=\"highlighted\">{addWord}</span>";
                }
                if (context.Length == 0) {
                    context = addWord;
                } else {
                    context += " " + addWord;
                }
            }
        }
        return context;
    }

    public string categoryLabel(int categoryId)
    {
        using (var conn = db.GetConn()) {
            var sql = @"select category_label from presearch_category where category_id = @catid ";
            return conn.QueryFirstOrDefault<string>(sql, new { catid = categoryId });
        }
    }

    public IEnumerable<presearch_list> presearchList(int categoryId) {
        using (var conn = db.GetConn()) {
            //conn.Open();
            var sql = @"select * from presearch_list where category_id = @catid 
                            order by search_label";
            return conn.Query<presearch_list>(sql, new { catid = categoryId });
        }
    }

    public presearch_list getPresearch(int category_id, int search_id) {
        using (var conn = db.GetConn()) {
            var sql = $@"select * 
                            from presearch_list 
                            where category_id = @catid
                              and search_id = @sid";
            return conn.QueryFirstOrDefault<presearch_list>(sql, new 
                { catid = category_id, sid = search_id});
        }
    }

    public IEnumerable<file_issues> allNewsletters() {
        using (var conn = db.GetConn()) {
            var sql = $@"select * from file_issues"; 
            return conn.Query<file_issues>(sql);
        }
    }

    public IEnumerable<presearch_category> CategoryList() {
        using (var conn = db.GetConn()) {
            var sql = $@"select * from presearch_category"; 
            return conn.Query<presearch_category>(sql);
        }
    }

    public IEnumerable<presearch_list> PresearchList(int categoryId)
    {
        using (var conn = db.GetConn()) {
            var sql = $@"select * from presearch_list 
                            where category_id = @categoryId
                            order by search_id"; 
            return conn.Query<presearch_list>(sql, new {categoryId = categoryId});
        }
    }
}