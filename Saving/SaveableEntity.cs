using System.Collections.Generic;
using RPG.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Saving
{
  // Edit모드에서도(모든 모드) 실행 할 수 있게 한다.
  [ExecuteAlways]
  public class SaveableEntity : MonoBehaviour
  {
    [SerializeField] string uniqueIdentifire = "";
    static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

    public string GetUniqueIdentifire()
    {
      return uniqueIdentifire;
    }

    public object CaptureState()
    {
      Dictionary<string, object> state = new Dictionary<string, object>();
      foreach (ISaveable saveable in GetComponents<ISaveable>())
      {
        state[saveable.GetType().ToString()] = saveable.CaptureSate();
      }
      return state;
    }

    public void RestoreState(object state)
    {
      Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
      foreach (ISaveable saveable in GetComponents<ISaveable>())
      {
        string typeString = saveable.GetType().ToString();
        if (stateDict.ContainsKey(typeString))
        {
          saveable.RestoreSate(stateDict[typeString]);
        }
      }
    }
#if UNITY_EDITOR
    // Edit mode 일때는 scene이 바뀔때 1번씩 실행된다.
    private void Update()
    {
      if (Application.IsPlaying(gameObject)) return;

      // prefab에는 guid를 새로 생성하지 않는다. scene에 올라와있는 object에만 guid를 붙여줌.
      if (string.IsNullOrEmpty(gameObject.scene.path)) return;

      // edit모드에서 prefab을 상세하게 수정하기 위해 SerializedObject로 타입 변환 해준다.
      SerializedObject serializedObject = new SerializedObject(this);
      SerializedProperty property = serializedObject.FindProperty("uniqueIdentifire");

      // property.stringValue( guid )가 지정 안되어 있거나
      // property.stringValue( guid )가 지정되어 있지만, 유니크하지 않다면( 중복된게 있다면 ) 새로 만들어준다.
      // unique 하다면( 중복된게 없다면 ) 그냥 지정되어있는 그대로 사용한다.
      if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
      {
        property.stringValue = System.Guid.NewGuid().ToString();
        serializedObject.ApplyModifiedProperties();
      }

      globalLookup[property.stringValue] = this;
    }
#endif

    private bool IsUnique(string candidate)
    {
      // globalLookup에 해당 키로 등록된 객체가 없다면 유니크 하다.
      if (!globalLookup.ContainsKey(candidate)) return true;
      // 해당키로 등록된 객체가 있지만, 그것이 본인(객체 자신)이라면 유니크하다.
      if (globalLookup[candidate].Equals(this)) return true;
      // 해당키로 등록된 객체가 있고 그것이 본인이 아니지만, 그것이 삭제된객체( null )이라면
      // 삭제된 객체를 map에서 지운다. 그렇다면 본인은 유니크하게 된다.
      if (globalLookup[candidate] == null)
      {
        globalLookup.Remove(candidate);
        return true;
      }
      // out of data 혹은 무슨 이유로 해당 객체가 가지고 있는 키와 이 키가 다를 수도 있으니 확인해준다.
      if (!globalLookup[candidate].GetUniqueIdentifire().Equals(candidate))
      {
        globalLookup.Remove(candidate);
        return true;
      }
      return false;
    }

  }
}