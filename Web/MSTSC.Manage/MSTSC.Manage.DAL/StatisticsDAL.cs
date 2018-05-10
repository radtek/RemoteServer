﻿using Dapper;
using MSTSC.Manage.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MSTSC.Manage.DAL
{
    /// <summary>
    /// 报表统计
    /// </summary>
    public class StatisticsDAL
    {
        /// <summary>
        /// 统计所有仪器
        /// </summary>
        /// <param name="conditValue"></param>
        /// <param name="pagerInfo"></param>
        /// <param name="sortInfo"></param>
        /// <returns></returns>
        public dynamic StatisticsAllDevicesDAL(QueryConditionModel conditValue, PagerInfo pagerInfo, SortInfo sortInfo)
        {
            StringBuilder whereSql = new StringBuilder();
            var sql = @"SELECT d.DeviceName, d.SIM, d.SN, c.count_times_total, r.reagent_dil, r.reagent_lh, r.reagent_r2 FROM device_info d INNER JOIN ( SELECT SN FROM device_info {0} ";
            sql += " LIMIT " + (pagerInfo.PageSize * (pagerInfo.CurrenetPageIndex - 1)) + "," + pagerInfo.PageSize;
            sql += ") AS t USING (SN) LEFT JOIN blood_count c ON d.SN = c.device_sn LEFT JOIN blood_reagent r ON d.SN = r.device_sn;";

            if (!string.IsNullOrEmpty(conditValue.DeviceType))
            {
                whereSql.Append(@" AND DeviceType='" + conditValue.DeviceType + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ProductSeries))
            {
                whereSql.Append(@" AND ProductSeries='" + conditValue.ProductSeries + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ModelType))
            {
                whereSql.Append(@" AND ProductModel='" + conditValue.ModelType + "'");
            }

            if (!string.IsNullOrEmpty(whereSql.ToString()))
            {
                whereSql = new StringBuilder(" WHERE " + whereSql.ToString().Substring(4));
            }

            var SqlCondit = string.Format(sql, whereSql.ToString());
            using (var conn = new MySqlConnection(Global.strConn))
            {
                return conn.Query<dynamic>(SqlCondit).ToList();
            }
        }

        public int getDeviceCount(QueryConditionModel conditValue)
        {
            StringBuilder whereSql = new StringBuilder();
            string sql = @"SELECT count(1) FROM device_info d ";

            if (!string.IsNullOrEmpty(conditValue.DeviceType))
            {
                whereSql.Append(@" AND d.DeviceType='" + conditValue.DeviceType + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ProductSeries))
            {
                whereSql.Append(@" AND d.ProductSeries='" + conditValue.ProductSeries + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ModelType))
            {
                whereSql.Append(@" AND d.ProductModel='" + conditValue.ModelType + "'");
            }

            if (!string.IsNullOrEmpty(whereSql.ToString()))
            {
                sql += " WHERE " + whereSql.ToString().Substring(4);
            }

            using (var conn = new MySqlConnection(Global.strConn))
            {
                return conn.QuerySingle<int>(sql);
            }
        }

        public DataTable StatisticsByModelBLL(QueryConditionModel conditValue)
        {
            StringBuilder whereSql = new StringBuilder();
            var sql = @"SELECT SUM(count_times_wb_cbc) AS count_times_wb_cbc,SUM(count_times_wb_cbc_crp) AS count_times_wb_cbc_crp,SUM(count_times_wb_crp) AS count_times_wb_crp,
SUM(count_times_pd_cbc) AS count_times_pd_cbc,SUM(count_times_pd_cbc_crp) AS count_times_pd_cbc_crp,SUM(count_times_pd_crp) AS count_times_pd_crp
FROM device_info,blood_count WHERE SN = device_sn {0}";

            if (!string.IsNullOrEmpty(conditValue.DeviceType))
            {
                whereSql.Append(@" AND DeviceType='" + conditValue.DeviceType + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ProductSeries))
            {
                whereSql.Append(@" AND ProductSeries='" + conditValue.ProductSeries + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ModelType))
            {
                whereSql.Append(@" AND ProductModel='" + conditValue.ModelType + "'");
            }

            var SqlCondit = string.Format(sql, whereSql.ToString());
            using (var conn = new MySqlConnection(Global.strConn))
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(SqlCondit, conn);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds.Tables[0];
                //return conn.Query<dynamic>(SqlCondit).ToList();
            }
        }

        /// <summary>
        /// 按封闭试剂类型统计
        /// </summary>
        /// <param name="conditValue"></param>
        /// <returns></returns>
        public DataTable StatisticsByReagentTypeDAL(QueryConditionModel conditValue)
        {
            StringBuilder whereSql = new StringBuilder();
            string sql = @"select t.ReagentType as 试剂封闭类型,count(sn) as 机器数, sum(t.count_times_total) as 样本数,
                            sum(t.reagent_dil) as 消耗稀释液,sum(t.reagent_lh) as 消耗溶血剂,sum(t.reagent_r2) as '消耗CRP R2',0 as 机器数比例,0 as 样本数比例 
                            from (SELECT d.SN,case  when reagenttype='open' then '开放' when reagenttype='close' then '封闭' else reagenttype end ReagentType ,
                             IFNULL(bc.count_times_total,0) as count_times_total,IFNULL(br.reagent_dil,0) as reagent_dil,IFNULL(br.reagent_lh,0) as reagent_lh,
                            IFNULL(br.reagent_r2,0) as reagent_r2
                        from device_info d 
                             left JOIN blood_count bc on d.SN=bc.device_sn
                             LEFT JOIN blood_reagent br on d.SN = br.device_sn
                             where 1=1 {0}) t
                             group by t.ReagentType";

            if (!string.IsNullOrEmpty(conditValue.DeviceType))
            {
                whereSql.Append(@" AND DeviceType='" + conditValue.DeviceType + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ProductSeries))
            {
                whereSql.Append(@" AND ProductSeries='" + conditValue.ProductSeries + "'");
            }

            if (!string.IsNullOrEmpty(conditValue.ModelType))
            {
                whereSql.Append(@" AND ProductModel='" + conditValue.ModelType + "'");
            }

            var SqlCondit = string.Format(sql, whereSql.ToString());
            using (var conn = new MySqlConnection(Global.strConn))
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                return ds.Tables[0];

                //return conn.Query<dynamic>(SqlCondit).ToList();
            }
        }

    }
}