using UnityEngine;

namespace cs
{
    public static class ComGuiBinderBaseExtend
    {
		public static ComGuiBinder_Test ToGuiBinder_Test(this ComGuiBinderBase a_binderBase)
		{
			return a_binderBase as ComGuiBinder_Test;
		}

    }
}