using UnityEngine;

public interface IPlayerBindable
{
    void BindPlayer(GameObject playerRoot);
    void UnbindPlayer();
}