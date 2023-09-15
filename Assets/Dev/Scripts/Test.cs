using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dev
{
    public class Test : MonoBehaviour
    {
       
        [ContextMenu(nameof(Calculate))]
        private void Calculate()
        {
            Debug.Log($"Подсчитывание данных...");

            List<(string, float)> data = new List<(string, float)>(10);

            data.Add(("Это Кёниг, детка!", 1211.8f));
            data.Add(("КалиниградРад", 3621.7f));
            data.Add(("Подслушано в Калинграде", 4080f));
            data.Add(("Янтарный ДЛБ", 14830f));
            data.Add(("Типичный Калининград", 9880f));
            data.Add(("Очень важный Калининград", 3455.7f));
            data.Add(("Клопс", 2680.2f));
            data.Add(("Калининград.ru", 1878.3f));
            data.Add(("Новый Калининград", 1121.7f));
            data.Add(("Вести Калининград", 555.2f));
            data.Add(("Радио ГТРК Калининград", 2.9f));
            data.Add(("РБК Калининград", 152.5f));
            data.Add(("Каскад ТВ", 7573.5f));
            data.Add(("Правивительство Калининградской области", 6740f));
            data.Add(("Анонс 39 Калининград", 1097.5f));

            var orderedEnumerable = data.OrderByDescending(x => x.Item2).ToList();

            string output = "";
            
            for (var index = 0; index < orderedEnumerable.Count; index++)
            {
                (string, float) valueTuple = orderedEnumerable[index];

                output += $"Место {index + 1}. {valueTuple.Item1} - {valueTuple.Item2}\n";
            }

            FileStream fileStream = File.OpenWrite(@"C:\Users\nikit\Desktop\data.txt");

            using (fileStream)
            {
                fileStream.Write( Encoding.Default.GetBytes(output));
            }
            
        }
        
        
        
    }
}