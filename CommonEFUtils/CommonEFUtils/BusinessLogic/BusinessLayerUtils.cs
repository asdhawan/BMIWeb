using CommonUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CommonEFUtils {
    public static class BusinessLayerUtils {
        /// <summary>
        /// Build DataObjects Parameters HashTable Using reflection.
        /// </summary>
        /// <param name="InstanceObject"></param>
        /// <param name="ObjectParamsList"></param>
        /// <returns></returns>
        public static Hashtable GenerateQueryParams(object InstanceObject, List<string> ObjectParamsList) {
            Hashtable queryParams = new Hashtable();

            foreach (string sParam in ObjectParamsList) {
                PropertyInfo objBusinessProp = InstanceObject.GetType().GetProperty(sParam);

                //Check for null values
                object oValue = objBusinessProp.GetValue(InstanceObject, null);
                if (!Utils.NullEmptyCheck(oValue))
                    queryParams.Add(sParam, objBusinessProp.PropertyType.ToString().Contains("Enum") ? oValue.ToString() : oValue);
                else
                    queryParams.Add(sParam, System.DBNull.Value);
            }

            return queryParams;
        }

        /// <summary>
        /// Populate instance of object from datarow data passed.
        /// </summary>
        /// <typeparam name="BO"></typeparam>
        /// <param name="oBuisnessObject"></param>
        /// <param name="dr"></param>
        public static void PopulateObject<BO>(BO oBuisnessObject, DataRow dr) where BO : new() {
            foreach (PropertyInfo oProperty in oBuisnessObject.GetType().GetProperties()) {
                try {
                    //Check if data row containes column
                    if (dr.Table.Columns.Contains(oProperty.Name) && dr[oProperty.Name] != null) {
                        SetPropertyValue<BO>(oProperty, oBuisnessObject, dr[oProperty.Name]);
                    }
                } catch {/*skip any errors*/ }
            }
        }

        /// <summary>
        /// Populate instance of object from datarow data passed.
        /// </summary>
        /// <typeparam name="BO"></typeparam>
        /// <param name="oBuisnessObject"></param>
        /// <param name="dr"></param>
        public static BO PopulateObject<BO>(DataRow dr) where BO : new() {
            BO oBuisnessObject = new BO();
            foreach (PropertyInfo oProperty in oBuisnessObject.GetType().GetProperties()) {
                try {
                    //Check if data row containes column
                    if (dr.Table.Columns.Contains(oProperty.Name) && dr[oProperty.Name] != null) {
                        SetPropertyValue(oProperty, oBuisnessObject, dr[oProperty.Name]);
                    }
                } catch { oBuisnessObject = default(BO); }
            }
            return oBuisnessObject;
        }


        /// <summary>
        /// Populate instance of object from datarow data passed.
        /// Optimized for fast data retrival and used in DataGrid population.
        /// </summary>
        /// <typeparam name="BO"></typeparam>
        /// <param name="oBuisnessObject"></param>
        /// <param name="dr"></param>
        public static BO PopulateObject<BO>(DataRow dr, List<string> DBColumns) where BO : new() {
            BO oBuisnessObject = new BO();
            foreach (string item in DBColumns) {
                try {
                    if (dr.Table.Columns.Contains(item) && dr[item] != null && dr[item] != DBNull.Value) {
                        PropertyInfo objBusinessProp = oBuisnessObject.GetType().GetProperty(item);
                        SetPropertyValue(objBusinessProp, oBuisnessObject, dr[item]);
                    }
                } catch { oBuisnessObject = default(BO); }
            }
            return oBuisnessObject;
        }

        /// <summary>
        /// Populate instance of object from datarow data passed.
        /// Optimized for fast data retrival and used in DataGrid population.
        /// </summary>
        /// <typeparam name="BO"></typeparam>
        /// <param name="oBuisnessObject"></param>
        /// <param name="dr"></param>
        public static void PopulateObject<BO>(BO oBuisnessObject, DataRow dr, List<string> DBColumns) where BO : new() {
            foreach (string item in DBColumns) {
                try {
                    if (dr.Table.Columns.Contains(item) && dr[item] != null) {
                        PropertyInfo objBusinessProp = oBuisnessObject.GetType().GetProperty(item);
                        SetPropertyValue(objBusinessProp, oBuisnessObject, dr[item]);
                    }
                } catch {/*skip any errors*/ }
            }
        }


        /// <summary>
        /// Populate instance of object from data passed.
        /// </summary>
        /// <typeparam name="BO"></typeparam>
        /// <param name="oBuisnessObject"></param>
        /// <param name="dr"></param>
        public static void PopulateObject<BO>(BO oBuisnessObject, IDictionary dr) where BO : new() {
            foreach (PropertyInfo oProperty in oBuisnessObject.GetType().GetProperties()) {
                try {
                    //Check if data row containes column
                    if (dr.Contains(oProperty.Name) && dr[oProperty.Name] != null) {
                        SetPropertyValue(oProperty, oBuisnessObject, dr[oProperty.Name]);
                    }
                } catch {/*skip any errors*/ }
            }
        }

        /// <summary>
        /// Populate instance of object from data passed.
        /// </summary>
        /// <typeparam name="BO"></typeparam>
        /// <param name="oBuisnessObject"></param>
        /// <param name="dr"></param>
        public static BO PopulateObject<BO>(IDictionary dr) where BO : new() {
            BO oBuisnessObject = new BO();
            foreach (PropertyInfo oProperty in oBuisnessObject.GetType().GetProperties()) {
                try {
                    //Check if data row containes column
                    if (dr.Contains(oProperty.Name) && dr[oProperty.Name] != null) {
                        SetPropertyValue(oProperty, oBuisnessObject, dr[oProperty.Name]);
                    }
                } catch { oBuisnessObject = default(BO); }
            }
            return oBuisnessObject;
        }
        public static E PopulateEntity<E, O>(O businessObject) where E : new() where O : new() {
            E entityObject = new E();
            Dictionary<string, PropertyInfo> entityObjectPropertiesDict = entityObject.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            foreach (PropertyInfo piBusinessObject in businessObject.GetType().GetProperties()) {
                try {
                    PropertyInfo piEntityObject = null;
                    if (entityObjectPropertiesDict.TryGetValue(piBusinessObject.Name, out piEntityObject) && piEntityObject != null) {
                        object value = piBusinessObject.GetValue(businessObject, null);
                        Type boPropType = piBusinessObject.PropertyType;
                        Type eoPropType = piEntityObject.PropertyType;
                        if (boPropType.IsEnum && eoPropType == typeof(int))
                            SetPropertyValue(piEntityObject, entityObject, Convert.ToInt32(value));
                        else if (IsNullableEnum(boPropType) && GetNullableBaseType(eoPropType) == typeof(int))
                            SetPropertyValue(piEntityObject, entityObject, value == null ? (int?)null : Convert.ToInt32(value));
                        else
                            SetPropertyValue(piEntityObject, entityObject, value);
                    }
                } catch { entityObject = default(E); }
            }
            return entityObject;
        }

        public static E PopulateEntity<E, O>(O businessObject, List<string> DBColumns) where E : new() where O : new() {
            E entityObject = new E();
            Dictionary<string, PropertyInfo> businessObjectPropertiesDict = businessObject.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            Dictionary<string, PropertyInfo> entityObjectPropertiesDict = entityObject.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            foreach (string columnName in DBColumns) {
                try {
                    PropertyInfo piBusinessObject = null;
                    PropertyInfo piEntityObject = null;
                    if (businessObjectPropertiesDict.TryGetValue(columnName, out piBusinessObject) &&
                        entityObjectPropertiesDict.TryGetValue(columnName, out piEntityObject) &&
                        piBusinessObject != null &&
                        piEntityObject != null) {
                        object value = piBusinessObject.GetValue(businessObject, null);
                        if (piBusinessObject.PropertyType.IsEnum && piEntityObject.PropertyType == typeof(int))
                            SetPropertyValue<E>(piEntityObject, entityObject, Convert.ToInt32(value));
                        else
                            SetPropertyValue<E>(piEntityObject, entityObject, value);
                    }
                } catch { entityObject = default(E); }
            }
            return entityObject;
        }

        public static O PopulateObject<O, E>(E entityObject) where O : new() {
            O businessObject = new O();
            Dictionary<string, PropertyInfo> businessObjectPropertiesDict = businessObject.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            foreach (PropertyInfo piEntityObject in entityObject.GetType().GetProperties()) {
                try {
                    PropertyInfo piBusinessObject = null;
                    if (businessObjectPropertiesDict.TryGetValue(piEntityObject.Name, out piBusinessObject) && piBusinessObject != null)
                        SetPropertyValue(piBusinessObject, businessObject, piEntityObject.GetValue(entityObject, null));
                } catch { businessObject = default(O); }
            }
            return businessObject;
        }

        public static O PopulateObject<O, E>(E entityObject, List<string> DBColumns) where O : new() {
            O businessObject = new O();
            Dictionary<string, PropertyInfo> businessObjectPropertiesDict = businessObject.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            Dictionary<string, PropertyInfo> entityObjectPropertiesDict = entityObject.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            foreach (string columnName in DBColumns) {
                try {
                    PropertyInfo piBusinessObject = null;
                    PropertyInfo piEntityObject = null;
                    if (businessObjectPropertiesDict.TryGetValue(columnName, out piBusinessObject) &&
                        entityObjectPropertiesDict.TryGetValue(columnName, out piEntityObject) &&
                        piBusinessObject != null &&
                        piEntityObject != null)
                        SetPropertyValue(piBusinessObject, businessObject, piEntityObject.GetValue(entityObject, null));
                } catch { businessObject = default(O); }
            }
            return businessObject;
        }

        public static List<O> BuildCollectionFromEntitySet<O, E>(IEnumerable<E> entitySet, List<string> DBColumns = null) where O : new() {
            List<O> retCollection = new List<O>();
            if (entitySet != null && entitySet.Count() != 0) {
                foreach (E eo in entitySet) {
                    //Build collection
                    O newObj = DBColumns != null ? PopulateObject<O, E>(eo, DBColumns) : PopulateObject<O, E>(eo);
                    if (newObj != null)
                        retCollection.Add(newObj);
                }
            }
            return retCollection;
        }

        public static List<BO> BuildCollectionFromDataTable<BO>(DataTable dt) where BO : new() {
            List<BO> retCollection = new List<BO>();
            if (dt != null && dt.Rows.Count != 0) {
                foreach (DataRow row in dt.Rows) {
                    //Build collection
                    BO newObj = PopulateObject<BO>(row);
                    if (newObj != null)
                        retCollection.Add(newObj);
                }
            }
            return retCollection;
        }

        public static List<BO> BuildCollectionFromDataTable<BO>(DataTable dt, List<string> DBColumns) where BO : new() {
            List<BO> retCollection = new List<BO>();
            if (dt != null && dt.Rows.Count != 0) {
                foreach (DataRow row in dt.Rows) {
                    //Build collection
                    BO newObj = PopulateObject<BO>(row, DBColumns);
                    if (newObj != null)
                        retCollection.Add(newObj);
                }
            }
            return retCollection;
        }

        public static void SetPropertyValue<T>(PropertyInfo oProperty, T oBusinessObject, object sValue) {
            Type targetType = IsNullableType(oProperty.PropertyType) ? Nullable.GetUnderlyingType(oProperty.PropertyType) : oProperty.PropertyType;
            if (targetType.IsEnum)
                oProperty.SetValue(oBusinessObject, sValue == null ? null : Enum.Parse(targetType, sValue.ToString()), null);
            else
                oProperty.SetValue(oBusinessObject, sValue == null ? null : Convert.ChangeType(sValue, targetType), null);
        }

        private static bool IsNullableEnum(Type type) { return GetNullableBaseType(type).IsEnum; }

        private static Type GetNullableBaseType(Type type) {
            Type retType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                retType = type.GetGenericArguments()[0];
            return retType;
        }

        private static bool IsNullableType(Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}
