using System.Collections;
using System.Collections.Generic;
using System.Data;
using BaiRong.Core.Data;
using BaiRong.Core.Model;
using SiteServer.CMS.Model;
using SiteServer.Plugin.Models;

namespace SiteServer.CMS.Provider
{
    public class RelatedFieldItemDao : DataProviderBase
    {
        public override string TableName => "siteserver_RelatedFieldItem";

        public override List<TableColumnInfo> TableColumns => new List<TableColumnInfo>
        {
            new TableColumnInfo
            {
                ColumnName = nameof(RelatedFieldItemInfo.Id),
                DataType = DataType.Integer,
                IsIdentity = true,
                IsPrimaryKey = true
            },
            new TableColumnInfo
            {
                ColumnName = nameof(RelatedFieldItemInfo.RelatedFieldId),
                DataType = DataType.Integer
            },
            new TableColumnInfo
            {
                ColumnName = nameof(RelatedFieldItemInfo.ItemName),
                DataType = DataType.VarChar,
                Length = 255
            },
            new TableColumnInfo
            {
                ColumnName = nameof(RelatedFieldItemInfo.ItemValue),
                DataType = DataType.VarChar,
                Length = 255
            },
            new TableColumnInfo
            {
                ColumnName = nameof(RelatedFieldItemInfo.ParentId),
                DataType = DataType.Integer
            },
            new TableColumnInfo
            {
                ColumnName = nameof(RelatedFieldItemInfo.Taxis),
                DataType = DataType.Integer
            }
        };

        private const string SqlUpdate = "UPDATE siteserver_RelatedFieldItem SET ItemName = @ItemName, ItemValue = @ItemValue WHERE ID = @ID";

        private const string ParmId = "@ID";
        private const string ParmRelatedFieldId = "@RelatedFieldID";
        private const string ParmItemName = "@ItemName";
        private const string ParmItemValue = "@ItemValue";
        private const string ParmParentId = "@ParentID";
        private const string ParmTaxis = "@Taxis";

        public int Insert(RelatedFieldItemInfo info)
        {
            info.Taxis = GetMaxTaxis(info.ParentId) + 1;

            const string sqlString = "INSERT INTO siteserver_RelatedFieldItem (RelatedFieldID, ItemName, ItemValue, ParentID, Taxis) VALUES (@RelatedFieldID, @ItemName, @ItemValue, @ParentID, @Taxis)";

            var parms = new IDataParameter[]
			{
                GetParameter(ParmRelatedFieldId, DataType.Integer, info.RelatedFieldId),
                GetParameter(ParmItemName, DataType.VarChar, 255, info.ItemName),
                GetParameter(ParmItemValue, DataType.VarChar, 255, info.ItemValue),
				GetParameter(ParmParentId, DataType.Integer, info.ParentId),
                GetParameter(ParmTaxis, DataType.Integer, info.Taxis)
			};

            return ExecuteNonQueryAndReturnId(TableName, nameof(RelatedFieldItemInfo.Id), sqlString, parms);

            //RelatedFieldManager.ClearCache();
        }

        public void Update(RelatedFieldItemInfo info)
        {
            var parms = new IDataParameter[]
			{
				GetParameter(ParmItemName, DataType.VarChar, 255, info.ItemName),
                GetParameter(ParmItemValue, DataType.VarChar, 255, info.ItemValue),
				GetParameter(ParmId, DataType.Integer, info.Id)
			};

            ExecuteNonQuery(SqlUpdate, parms);

            //RelatedFieldManager.ClearCache();
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                string sqlString = $"DELETE FROM siteserver_RelatedFieldItem WHERE ID = {id} OR ParentID = {id}";
                ExecuteNonQuery(sqlString);
            }
            //RelatedFieldManager.ClearCache();
        }

        public List<RelatedFieldItemInfo> GetRelatedFieldItemInfoList(int relatedFieldId, int parentId)
        {
            var list = new List<RelatedFieldItemInfo>();

            string sqlString =
                $"SELECT ID, RelatedFieldID, ItemName, ItemValue, ParentID, Taxis FROM siteserver_RelatedFieldItem WHERE RelatedFieldID = {relatedFieldId} AND ParentID = {parentId} ORDER BY Taxis";

            using (var rdr = ExecuteReader(sqlString))
            {
                while (rdr.Read())
                {
                    var i = 0;
                    var info = new RelatedFieldItemInfo(GetInt(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetInt(rdr, i));
                    list.Add(info);
                }
                rdr.Close();
            }

            return list;
        }

        public void UpdateTaxisToUp(int id, int parentId)
        {
            //Get Higher Taxis and ClassID
            //string sqlString =
            //    $"SELECT TOP 1 ID, Taxis FROM siteserver_RelatedFieldItem WHERE ((Taxis > (SELECT Taxis FROM siteserver_RelatedFieldItem WHERE ID = {id})) AND ParentID = {parentId}) ORDER BY Taxis";
            var sqlString = SqlUtils.GetTopSqlString("siteserver_RelatedFieldItem", "ID, Taxis", $"WHERE ((Taxis > (SELECT Taxis FROM siteserver_RelatedFieldItem WHERE ID = {id})) AND ParentID = {parentId})", "ORDER BY Taxis", 1);

            var higherId = 0;
            var higherTaxis = 0;

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    higherId = GetInt(rdr, 0);
                    higherTaxis = GetInt(rdr, 1);
                }
                rdr.Close();
            }

            //Get Taxis Of Selected Class
            var selectedTaxis = GetTaxis(id);

            if (higherId != 0)
            {
                //Set The Selected Class Taxis To Higher Level
                SetTaxis(id, higherTaxis);
                //Set The Higher Class Taxis To Lower Level
                SetTaxis(higherId, selectedTaxis);
            }

            //RelatedFieldManager.ClearCache();
        }

        public void UpdateTaxisToDown(int id, int parentId)
        {
            //Get Lower Taxis and ClassID
            //string sqlString =
            //    $"SELECT TOP 1 ID, Taxis FROM siteserver_RelatedFieldItem WHERE ((Taxis < (SELECT Taxis FROM siteserver_RelatedFieldItem WHERE (ID = {id}))) AND ParentID = {parentId}) ORDER BY Taxis DESC";
            var sqlString = SqlUtils.GetTopSqlString("siteserver_RelatedFieldItem", "ID, Taxis", $"WHERE ((Taxis < (SELECT Taxis FROM siteserver_RelatedFieldItem WHERE (ID = {id}))) AND ParentID = {parentId})", "ORDER BY Taxis DESC", 1);

            var lowerId = 0;
            var lowerTaxis = 0;

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    lowerId = GetInt(rdr, 0);
                    lowerTaxis = GetInt(rdr, 1);
                }
                rdr.Close();
            }

            //Get Taxis Of Selected Class
            var selectedTaxis = GetTaxis(id);

            if (lowerId != 0)
            {
                //Set The Selected Class Taxis To Lower Level
                SetTaxis(id, lowerTaxis);
                //Set The Lower Class Taxis To Higher Level
                SetTaxis(lowerId, selectedTaxis);
            }

            //RelatedFieldManager.ClearCache();
        }

        private int GetTaxis(int id)
        {
            string cmd = $"SELECT Taxis FROM siteserver_RelatedFieldItem WHERE (ID = {id})";
            var taxis = 0;

            using (var rdr = ExecuteReader(cmd))
            {
                if (rdr.Read())
                {
                    taxis = GetInt(rdr, 0);
                }
                rdr.Close();
            }
            return taxis;
        }

        private void SetTaxis(int id, int taxis)
        {
            string cmd = $"UPDATE siteserver_RelatedFieldItem SET Taxis = {taxis} WHERE ID = {id}";

            ExecuteNonQuery(cmd);
        }

        public int GetMaxTaxis(int parentId)
        {
            int maxTaxis;
            string cmd =
                $"SELECT MAX(Taxis) FROM siteserver_RelatedFieldItem WHERE ParentID = {parentId} AND Taxis <> {int.MaxValue}";
            using (var conn = GetConnection())
            {
                conn.Open();
                var o = ExecuteScalar(conn, cmd);
                if (o is System.DBNull)
                    maxTaxis = 0;
                else
                    maxTaxis = int.Parse(o.ToString());
            }
            return maxTaxis;
        }

        public int GetMinTaxis(int parentId)
        {
            int minTaxis;
            string cmd = $"SELECT MIN(Taxis) FROM siteserver_RelatedFieldItem WHERE ParentID = {parentId}";
            using (var conn = GetConnection())
            {
                conn.Open();
                var o = ExecuteScalar(conn, cmd);
                if (o is System.DBNull)
                    minTaxis = 0;
                else
                    minTaxis = int.Parse(o.ToString());
            }
            return minTaxis;
        }

        public RelatedFieldItemInfo GetRelatedFieldItemInfo(int id)
        {
            RelatedFieldItemInfo info = null;

            string sqlString =
                $"SELECT ID, RelatedFieldID, ItemName, ItemValue, ParentID, Taxis FROM siteserver_RelatedFieldItem WHERE ID = {id}";

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    var i = 0;
                    info = new RelatedFieldItemInfo(GetInt(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetInt(rdr, i));
                }
                rdr.Close();
            }

            return info;
        }

        
    }
}