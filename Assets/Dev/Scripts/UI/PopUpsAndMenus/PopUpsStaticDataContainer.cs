using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.UI.PopUpsAndMenus
{
    [CreateAssetMenu(menuName = "StaticData/PopUpsPrefabs", fileName = "PopUpsStaticDataContainer", order = 0)]
    public class PopUpsStaticDataContainer : ScriptableObject
    {
        [SerializeField] private List<PopUp> _popups;

        public T GetPrefab<T>() where T : PopUp
        {
            Type type = typeof(T);

            return _popups.FirstOrDefault(x => x.GetType() == type) as T;
        }
    }
}