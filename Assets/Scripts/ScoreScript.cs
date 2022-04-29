using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*

THIS SCRIPT HAS EVERYTHING THAT DEALS WITH SCORE KEEPING IN THE GAME.
by Tyler McMillan
*/
public class ScoreScript : MonoBehaviour
{
    private int _didYouWinMP = 0; //if 0 you lost, if 1 you won

    int _winnerNumber = -1;
    private int[] _playerScores = new int[] { 0, 0, 0, 0 }; //player 1-4 scores (how many boxes have they collected)
    [SerializeField] private Text _winnerText; //text that displays who won the game or if it was a tie
    [SerializeField] private Animator _endgameScoreAnimator; //animator on engame score screen (changes size of scorebox depending on how many players)
    [SerializeField] private Text[] _playerScoreTextboxes; //player 1 - 4 score text boxes
    [SerializeField] private Text[] _playerNameTextboxes; //player 1-4 name text boxes in score (during game score screen.)
    [SerializeField] private Text[] _endgameScoreTextboxes; //endgame 1 - 4 place score text boxes

    [SerializeField] private GameObject _gameoverScoreboardObj; //gameover obj displays score screen and restart button
    [SerializeField] private GameObject _LCRestart, _MPRestart; //gameobjects that hold restart buttons for multiplayer or localplay

    public void UpdateGameScore(int m_numberOfPlayers) //update scoreboard (ingame)
    {
        for (int i = 0; i < m_numberOfPlayers; i++)
        {
            _playerScoreTextboxes[i].text = _playerScores[i].ToString(); //display score while game is running
        }
    }
    public void UpdateUserNames(int m_playerCtr, string m_playerName) //Update player names in scoreboard (ingame)
    {
        _playerNameTextboxes[m_playerCtr].text = m_playerName + ": ";
    }
    public void AddScore(int m_playerCtr) //add to players score
    {
        _playerScores[m_playerCtr]++;
    }
    public void RestartScore(int m_playerCtr) //restart ingame scoreboard obj and other scoreboard variables (INSIDE FOR LOOP)
    {
        _playerScores[m_playerCtr] = 0; //set scores to 0
        _playerScoreTextboxes[m_playerCtr].text = _playerScores[m_playerCtr].ToString(); //display scores in text box
        if (m_playerCtr == 0)//do only once;
        { 
            _gameoverScoreboardObj.SetActive(false); //make gameover panel invisible.
        }
    }
    public void MPGameoverMessage(SocketManager m_socketManager, int m_numberOfPlayers, bool m_localGame, int m_myMPNum, string[] m_playerNames) //called when multiplayer game is over, or someone leaves when game is atleast 50% done.
    {
        DisplayGameoverScore(m_numberOfPlayers, m_localGame); //display gameover scoreboard
        CalculateWinner(m_numberOfPlayers, m_myMPNum, m_playerNames); //calculate winner of the game // update scoreboard text boxes with scores. in order 1st-4th
        UpdateGameScore(m_numberOfPlayers); //update ingame scoreboard numbers.
        m_socketManager.SendGameoverMessage(_didYouWinMP); //send gameover message to the server.
    }
    public void HideScoreBoard() //hide game over scoreboard (on reset) otherwise its hidden elsewhere
    {
        _gameoverScoreboardObj.SetActive(false);
    }
    public void DisplayGameoverScore(int m_numberOfPlayers, bool m_localGame)//show end game scoreboard
    {
        _LCRestart.SetActive(false);
        _MPRestart.SetActive(false);
        if (m_localGame)
        {
            _LCRestart.SetActive(true);
        }
        else if (!m_localGame)
        {
            _MPRestart.SetActive(true);
        }

        _endgameScoreAnimator.SetInteger("NumberOfPlayers", m_numberOfPlayers); //change size of score screen depending on player number
        _gameoverScoreboardObj.SetActive(true); //show the endgame score screen and the restart btn
    }
    public void CalculateWinner(int m_numberOfPlayers, int m_myMPNum, string[] m_playerNames) //calculate Score screen scores. && update endgame scoreboard textboxes
    {
        _didYouWinMP = 0; //reset if you won so you get acurate reading
        int[] m_playerPlace = new int[] { 0, 0, 0, 0 }; //check what place each player finished in.
        for (int i = 0; i < m_numberOfPlayers; i++)
        {
            for (int x = 0; x < m_numberOfPlayers; x++)
            {
                if (i != x)
                {
                    if (_playerScores[i] > _playerScores[x]) //if a player has a greater score than another, add points to their score.
                    {
                        m_playerPlace[i]++;
                    }
                }
            }
        }
        int m_firstPlace = -1; //whos in first place, if its -1 no ones in first.
        List<int> m_secondPlace = new List<int>();
        List<int> m_thirdPlace = new List<int>();
        List<int> m_fourthPlace = new List<int>();
        for (int i = 0; i < m_numberOfPlayers; i++)
        {
            //set players places
            if (m_playerPlace[i] == 3)
            {
                m_firstPlace = i;
                if (i == m_myMPNum && m_numberOfPlayers == 4)
                {
                    _didYouWinMP = 1;
                }
            }
            if (m_playerPlace[i] == 2)
            {
                m_secondPlace.Add(i);
                if (i == m_myMPNum && m_numberOfPlayers == 3)
                {
                    _didYouWinMP = 1;
                }
            }
            if (m_playerPlace[i] == 1)
            {
                m_thirdPlace.Add(i);
                if (i == m_myMPNum && m_numberOfPlayers == 2)
                {
                    _didYouWinMP = 1;
                }
            }
            if (m_playerPlace[i] == 0)
            {
                m_fourthPlace.Add(i);
            }

        }
        if (m_firstPlace == -1) //if its a 4player game and -1 its a tie.
        {
            _winnerText.text = "Tie Game!";
        }

        int m_ctr = 0;
        //check that you have a player in first (if 4 player)
        if (m_firstPlace != -1)
        {
            _endgameScoreTextboxes[m_ctr].text = m_playerNames[m_firstPlace] + "      Boxes: " + _playerScores[m_firstPlace];
            _winnerText.text = m_playerNames[m_firstPlace] + " is the Winner!";
            _winnerNumber = m_firstPlace;
            m_ctr++;
        }
        foreach (int second in m_secondPlace) //cycle through any players that tied for second
        {
            _endgameScoreTextboxes[m_ctr].text = m_playerNames[second] + "      Boxes: " + _playerScores[second];
            if (m_firstPlace == -1 && m_secondPlace.Count == 1)
            {
                _winnerText.text = m_playerNames[second] + " is the Winner!";
                _winnerNumber = second;
            }
            m_ctr++;
        }
        foreach (int third in m_thirdPlace)//cycle through any players that tied for third
        {
            _endgameScoreTextboxes[m_ctr].text = m_playerNames[third] + "      Boxes: " + _playerScores[third];
            if (m_secondPlace.Count == 0 && m_thirdPlace.Count == 1)
            {
                _winnerText.text = m_playerNames[third] + " is the Winner!";
                _winnerNumber = third;
            }
            m_ctr++;
        }
        foreach (int fourth in m_fourthPlace)//cycle through any players that tied for fourth
        {
            _endgameScoreTextboxes[m_ctr].text = m_playerNames[fourth] + "      Boxes: " + _playerScores[fourth];
            m_ctr++;
        }

    }
    public int GetEarlyWinner(int m_boardSize, int m_numberOfPlayers, int m_myMPNum, string[] m_playerNames)//Get winner id if there is one and someone left early.
    {
        int m_winnerNum = -1;
        int totalScore = 0;
        foreach (int m_score in _playerScores)
        {
            totalScore += m_score;
        }
        if (totalScore >= ((m_boardSize * m_boardSize) / 2)) //if you have collected over half of the games squares change from -1 to something else.
        {
            CalculateWinner(m_numberOfPlayers, m_myMPNum, m_playerNames);
            m_winnerNum = _winnerNumber;

        }
        Debug.Log("the winner is " + m_winnerNum);
        return m_winnerNum;

    }
}
