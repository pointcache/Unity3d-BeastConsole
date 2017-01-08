using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//Copy this file in your own game folder and modify to add new rVar types
//Replace TYPE with your type and then add it to CustomRVarDrawers.cs file

/*
[Serializable]
public class r_TYPE : rVar<TYPE>
{
    public r_TYPE() : base() {}
    public r_TYPE(TYPE initialValue): base(initialValue){}
    public static implicit operator TYPE(r_TYPE var) {  return var.Value; }

}
*/