using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Player.Customization.Data
{
    [CreateAssetMenu(fileName = "AvatarCustomizationCollection", menuName = "Playcebo/Avatar Customization Collection", order = 0)]
    public class AvatarCustomizationCollection : ScriptableObject
    {
        [SerializeField] private List<AvatarCustomizationItem> _avatarItems = new();
        public List<AvatarCustomizationItem> AvatarItems => _avatarItems;
        
        public AvatarCustomizationItem GetAvatarItem(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
                return null;

            return _avatarItems.Find(item => item != null && item.AvatarId == avatarId);
        }

        public AvatarCustomizationItem GetNextLevel(string currentLevelId)
        {
            int currentIndex = _avatarItems.FindIndex(item => item != null && item.AvatarId == currentLevelId);
            if (currentIndex < 0 || currentIndex + 1 >= _avatarItems.Count)
                return null;

            return _avatarItems[currentIndex + 1];
        }

        public IReadOnlyList<AvatarCustomizationItem> GetAllLevels()
        {
            return _avatarItems.AsReadOnly();
        }
    }
}