using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountStatScript : MonoBehaviour
{
    [SerializeField] private Text _usernameTextbox, _winsTextbox, _winLossRatioTextbox, _matchesTextbox;
    private float _winLossRatio = 0;

    public void UpdateAccountStats(string m_user, int m_wins, int m_matchesTotal){
        _winLossRatio = 0; //Reset for next time.
        if(m_matchesTotal != 0){
            _winLossRatio = (m_wins/m_matchesTotal) * 100;//find win loss ratio %
        }
        _usernameTextbox.text = m_user;
        _winsTextbox.text = m_wins.ToString();
        _winLossRatioTextbox.text = _winLossRatio.ToString("F2") + "%";
        _matchesTextbox.text = m_matchesTotal.ToString();
        
    }
}
