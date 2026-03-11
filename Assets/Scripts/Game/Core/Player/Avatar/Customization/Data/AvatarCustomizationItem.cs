using UnityEditor;
using UnityEngine;

namespace Game.Core.Player.Customization.Data
{
    [CreateAssetMenu(fileName = "AvatarCustomizationItem", menuName = "Playcebo/Avatar Customization Item", order = 0)]
    public class AvatarCustomizationItem : ScriptableObject
    {
        [SerializeField] private string _avatarId;
        public string AvatarId => _avatarId;
        
        [SerializeField] private string _avatarName;
        public string AvatarName => _avatarName;
        
        [SerializeField] private Sprite _avatarSprite;
        public Sprite AvatarSprite => _avatarSprite;
        
        [SerializeField] private GameObject _avatarPrefab;
        public GameObject AvatarPrefab => _avatarPrefab;
            
#if UNITY_EDITOR
        private void Reset()
        {
            GenerateIdIfNeeded();
        }

        private void OnValidate()
        {
            GenerateIdIfNeeded();
        }

        private void GenerateIdIfNeeded()
        {
            if (string.IsNullOrEmpty(_avatarId))
            {
                _avatarId = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}