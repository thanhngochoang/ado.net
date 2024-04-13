using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Tony.SampeAdo.Infastructure.Extentions
{
    public class DynamicBuilder<T>
    {
        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new[] { typeof(int) });

        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new[] { typeof(int) });

        private delegate T Load(IDataRecord dataRecord);

        private Load _handler;

        private DynamicBuilder()
        {
        }

        public T Build(IDataRecord dataRecord)
        {
            return _handler(dataRecord);
        }

        public static DynamicBuilder<T> CreateBuilder(IDataRecord dataRecord)
        {
            var dynamicBuilder = new DynamicBuilder<T>();

            var method = new DynamicMethod("DynamicCreate", typeof(T), new[] { typeof(IDataRecord) },
                                                     typeof(T), true);
            var generator = method.GetILGenerator();

            var result = generator.DeclareLocal(typeof(T));
            if (IsSingleType(typeof(T)))
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, 0);
                generator.Emit(OpCodes.Callvirt, getValueMethod);
                generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(0));

                generator.Emit(OpCodes.Ret);
            }
            else
            {
                generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                generator.Emit(OpCodes.Stloc, result);
                for (var i = 0; i < dataRecord.FieldCount; i++)
                {
                    var propertyInfo = typeof(T).GetProperty(dataRecord.GetName(i), BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    var endIfLabel = generator.DefineLabel();

                    if (propertyInfo == null)
                        continue;
                    var proMethod = propertyInfo.GetSetMethod();
                    if (proMethod == null)
                        continue;
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                    generator.Emit(OpCodes.Brtrue, endIfLabel);

                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, getValueMethod);
                    generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                    generator.Emit(OpCodes.Callvirt, proMethod);

                    generator.MarkLabel(endIfLabel);
                }

                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ret);
            }

            dynamicBuilder._handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }

        private static bool IsSingleType(Type type)
        {
            return type.IsValueType || type.Name == "String";
        }
    }
}
