using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FakeApi.Helper
{
	public class ArrayModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			// 我們綁定工作只在 EnumerableType 上
			if (!bindingContext.ModelMetadata.IsEnumerableType)
			{
				bindingContext.Result = ModelBindingResult.Failed();
				return Task.CompletedTask;
			}

			// 透過 ValueProvider 來取的數據
			var value = bindingContext.ValueProvider
				.GetValue(bindingContext.ModelName).ToString();

			// 判斷數據是否為空
			if(string.IsNullOrWhiteSpace(value))
			{
				bindingContext.Result = ModelBindingResult.Success(null);
				return Task.CompletedTask;
			}

			// 如果數值不為空 & 類型正確
			// 轉換數值
			var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
			var converter = TypeDescriptor.GetConverter(elementType); // 類型轉換

			// 切割數據&轉換數據加入到數組中
			var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => converter.ConvertFromString(x.Trim()))
				.ToArray();

			// 反射數據到typeValues
			var typeValues = Array.CreateInstance(elementType, values.Length);
			values.CopyTo(typeValues, 0);
			bindingContext.Model = typeValues;

			// 回傳成功結果
			bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
			return Task.CompletedTask;

		}
	}
}
