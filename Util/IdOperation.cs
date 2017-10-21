using System.Collections.Generic;

namespace com.caijunxiong.util
{
    /// <summary>
    /// Id操作,格式如下"1,2,3"
    /// </summary>
    public class IdOperation
    {
        #region

        /// <summary>
        /// 判断是否包含要查找的ID
        /// </summary>
        /// <param name="idList">操作ID列表</param>
        /// <param name="id">要查找的ID</param>
        /// <returns></returns>
        public static bool HasId(string idList, string id)
        {
            string[] ids = idList.Split(',');
            bool has = false;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == id)
                {
                    has = true;
                    break;
                }
            }
            return has;
        }

        /// <summary>
        /// 返回满足给定ID的查询字符串(查找在idList中的model)
        /// </summary>
        /// <param name="fieldName">查找的字段</param>
        /// <param name="idList">ID列表</param>
        /// <returns></returns>
        public static string GetInListString(string fieldName, string idList)
        {

            string str = "  " + fieldName + " in (" + idList + ")  ";
            return str;
        }

        /// <summary>
        /// 返回满足给定ID的查询字符串(按ID查找)
        /// </summary>
        /// <param name="fieldName">查找的字段</param>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public static string GetSearchString(string fieldName, string id)
        {
            string str = " (" + fieldName + " = '" + id + "' or " + fieldName + " like '" + id + ",%' or " + fieldName + " like '%," + id + "' or " + fieldName + " like '%," + id + ",%') ";
            return str;
        }

        /// <summary>
        /// 返回满足给定ID的查询字符串
        /// </summary>
        /// <param name="fieldName">查找的字段</param>
        /// <param name="idList">ID列表</param>
        /// <returns></returns>
        public static string GetSearchStringByIdList(string fieldName, string idList)
        {
            string[] ids = idList.Split(',');
            string str = "";
            for (int i = 0; i < ids.Length; i++)
            {
                if (i < (ids.Length - 1))
                {
                    str += GetSearchString(fieldName, ids[i].ToString()) + "or";
                }
                else
                {
                    str += GetSearchString(fieldName, ids[i].ToString());
                }
            }
            return str;
        }

        /// <summary>
        /// 去除重复的ID
        /// </summary>
        /// <param name="idList">要清除重复ID的ID列表</param>
        /// <returns></returns>
        public static string GetDistinctIds(string idList)
        {
            string[] ids = idList.Split(',');
            List<string> list = new List<string>();
            string str = "" + ids[0];
            for (int i = 0; i < ids.Length; i++)
            {
                if (!list.Contains(ids[i]))
                {
                    list.Add(ids[i]);
                }
            }
            str = string.Join(",", list.ToArray());
            return str;
        }

        /// <summary>
        /// 从操作ID列表中添加所给的ID
        /// </summary>
        /// <param name="operationIds">被操作ID列表</param>
        /// <param name="addIds">要添加的ID列表</param>
        /// <returns></returns>
        public static string AddIds(string operationIds, string addIds)
        {
            if (string.IsNullOrEmpty(operationIds))
            {
                operationIds += addIds;
            }
            else if (operationIds.EndsWith(","))
            {
                operationIds += addIds;
            }
            else
            {
                if (!string.IsNullOrEmpty(addIds))
                {
                    operationIds += "," + addIds;
                }
            }
            string str = GetDistinctIds(operationIds);
            return str;
        }

        /// <summary>
        /// 从操作ID列表中删除所给的ID
        /// </summary>
        /// <param name="operationIds">操作ID列表</param>
        /// <param name="deleteIds">要删除的ID列表</param>
        /// <returns></returns>
        public static string DeleteIds(string operationIds, string deleteIds)
        {
            string[] parentIds = operationIds.Split(',');
            string[] ids = deleteIds.Split(',');

            List<string> list = new List<string>();
            list.AddRange(parentIds);

            string str = "";
            for (int i = 0; i < ids.Length; i++)
            {
                if (list.Contains(ids[i]))
                {
                    list.Remove(ids[i]);
                }
                /* //查找是否有重复项,如有,则清除重复项
                 bool has_same = false;
                 for (int j = 0; j < ids.Length; j++)
                 {
                     if (parentIds[i] == ids[j])
                     {
                         has_same = true;
                         break;
                     }
                 }
                 if (!has_same)
                 {

                     str += (str == "" ? "" : ",") + parentIds[i];
                 }*/
            }
            str = string.Join(",", list.ToArray());
            return str;
        }

        #endregion
    }
}
