// using Microsoft.VisualBasic.CompilerServices;
// 
// namespace tr.Database.SqlShared;
// 
// class UpdateDictionary<T>
// {
//     private Dictionary<string, T> m_values = null;
//     
//     public void SetField(string field, T value)
//     {
//         m_values[field] = value;
//     }
// 
//     public IReadOnlyCollection<string> Fields => m_values.Keys;
//     public T this[string index] => m_values[index];
// }
// 
// /*----------------------------------------------------------------------------
//     %%Class: UpdateValues
//     %%Qualified: tr.Database.SqlShared.UpdateValues
// 
//     Holds all the values that should be updated in an update statement
// ----------------------------------------------------------------------------*/
// public class UpdateValues
// {
//     private Dictionary<Type, object> m_updateValues = new();
//     private Dictionary<string, Type> m_fields = new();
// 
//     public void SetField<T>(string field, T value)
//     {
//         if (m_fields.TryGetValue(field, out Type? t))
//         {
//             if (t != typeof(T))
//                 throw new Exception($"Field {field} already exists with type {t.Name}");
//         }
// 
//         if (!m_updateValues.ContainsKey(typeof(T)))
//             m_updateValues[typeof(T)] = new UpdateDictionary<T>();
// 
//         UpdateDictionary<T> dict = (UpdateDictionary<T>)m_updateValues[typeof(T)];
//         dict.SetField(field, value);
//     }
// 
//     public IReadOnlyCollection<string> Fields => m_fields.Keys;
// 
//     public T GetField<T>(string field)
//     {
//         if (!m_updateValues.ContainsKey(typeof(T)))
//             throw new Exception($"Field {field} does not exist");
//         UpdateDictionary<T> dict = (UpdateDictionary<T>)m_updateValues[typeof(T)];
//         return dict[field];
//     }
// }
