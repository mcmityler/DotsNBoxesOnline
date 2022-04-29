using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*

THIS SCRIPT HAS EVERYTHING THAT DEALS WITH THE ACCOUNT SCREEN IN MULTIPLAYER
by Tyler McMillan
*/
public class AccountStatScript : MonoBehaviour
{
    [SerializeField] private Text _usernameTextbox, _winsTextbox, _winLossRatioTextbox, _matchesTextbox; //reference to textboxes on account screen that hold stats

    public void UpdateAccountStats(string m_user, int m_wins, int m_matchesTotal){
        float m_winLossRatio = 0; //float that holds the win loss ratio
        if(m_matchesTotal != 0){ //make sure you arent trying to divide 0/0
            m_winLossRatio = (m_wins/m_matchesTotal) * 100;//find win loss ratio %
        }
        _usernameTextbox.text = m_user; //update text in user text
        _winsTextbox.text = m_wins.ToString();//update text in win text
        _winLossRatioTextbox.text = m_winLossRatio.ToString("F2") + "%";//update text in win loss ratio text
        _matchesTextbox.text = m_matchesTotal.ToString();//update text in total matches text
        
    }
}
