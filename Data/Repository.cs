using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;
using Dapper;
using System.IO;

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

        var result = multiResult.OrderBy(a => a.filename).ThenBy(b => b.pagenum).ThenBy(c => c.linenum);
        return result;
    }

    private List<SearchResult> search(string search_term) {
        var words = search_term.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();

        var allResult = new List<SearchResult>();
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
                                where word_index.wordnum = search_result.wordnum + @idx
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

            var result_sql = "select * from search_result";
            var result_comm = conn.CreateCommand();
            result_comm.CommandText = result_sql;

            var reader = result_comm.ExecuteReader();
            while (reader.Read()) {

                var result = new SearchResult();

                //TODO: Need to call build_context here
                //orig: result.context = reader[2].ToString();
                result.context = buildContext(reader[0].ToString(), (int)reader[1]);
                
                result.filename = reader[0].ToString();
                result.basename = Path.GetFileName(result.filename);
                result.wordnum = (int)reader[1];
                result.pagenum = (int)reader[3];
                result.linenum = (int)reader[4];
                
                allResult.Add(result);
            }
        }

        return allResult;
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

            var result_sql = @"select search_result.* 
                                from search_result join file_issues using (filename)
                                order by issue_num, sub_issue";
            var result_comm = conn.CreateCommand();
            result_comm.CommandText = result_sql;

            var reader = result_comm.ExecuteReader();
            while (reader.Read()) {

                var result = new SearchResult();

                //TODO: Need to call build_context here
                //orig: result.context = reader[2].ToString();
                result.context = buildContext(reader[0].ToString(), (int)reader[1]);
                
                result.filename = reader[0].ToString();
                result.basename = Path.GetFileNameWithoutExtension(result.filename);
                result.wordnum = (int)reader[1];
                result.pagenum = (int)reader[3];
                result.linenum = (int)reader[4];
                result.description = (string)reader[5];
                
                allResult.Add(result);
            }
        }

        return allResult;
    }


    public string buildContext(string fileName, int wordNum) {
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
                if (context.Length == 0) {
                    context = reader[0].ToString();
                } else {
                    context += " " + reader[0].ToString();
                }
            }
        }
        return context;
    }

    public IEnumerable<presearch> presearchList(int categoryId) {
        using (var conn = db.GetConn()) {
            //conn.Open();
            var sql = @"select * from presearch_list where category_id = @catid 
                            order by search_label";
            return conn.Query<presearch>(sql, new { catid = categoryId });
        }
    }

    public presearch getPresearch(int category_id, int search_id) {
        using (var conn = db.GetConn()) {
            var sql = $@"select * 
                            from presearch_list 
                            where category_id = @catid
                              and search_id = @sid";
            return conn.QueryFirstOrDefault<presearch>(sql, new 
                { catid = category_id, sid = search_id});
        }
    }

    public IEnumerable<file_issues> allNewsletters() {
        using (var conn = db.GetConn()) {
            var sql = $@"select * from file_issues"; 
            return conn.Query<file_issues>(sql);
        }
    }
}