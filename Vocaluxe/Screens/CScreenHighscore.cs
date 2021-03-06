﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Vocaluxe.Base;
using Vocaluxe.GameModes;
using Vocaluxe.Lib.Song;
using Vocaluxe.Menu;

namespace Vocaluxe.Screens
{
    class CScreenHighscore : CMenu
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        const int ScreenVersion = 1;

        private const int NumEntrys = 10;
        private const string TextSongName = "TextSongName";
        private const string TextSongMode = "TextSongMode";
        private string[] TextNumber;
        private string[] TextName;
        private string[] TextScore;
        private string[] TextDate;

        private List<SScores> _Scores;
        private List<int> _NewEntryIDs;
        private int _Round;
        private int _Pos;
        private bool _IsDuet;

        public CScreenHighscore()
        {
            Init();
        }

        protected override void Init()
        {
            base.Init();

            _ThemeName = "ScreenHighscore";
            _ScreenVersion = ScreenVersion;

            List<string> texts = new List<string>();
            texts.Add(TextSongName);
            texts.Add(TextSongMode);

            TextNumber = new string[NumEntrys];
            for (int i = 0; i < NumEntrys; i++)
            {
                TextNumber[i] = "TextNumber" + (i + 1).ToString();
                texts.Add(TextNumber[i]);
            }

            TextName = new string[NumEntrys];
            for (int i = 0; i < NumEntrys; i++)
            {
                TextName[i] = "TextName" + (i + 1).ToString();
                texts.Add(TextName[i]);
            }

            TextScore = new string[NumEntrys];
            for (int i = 0; i < NumEntrys; i++)
            {
                TextScore[i] = "TextScore" + (i + 1).ToString();
                texts.Add(TextScore[i]);
            }

            TextDate = new string[NumEntrys];
            for (int i = 0; i < NumEntrys; i++)
            {
                TextDate[i] = "TextDate" + (i + 1).ToString();
                texts.Add(TextDate[i]);
            }

            _ThemeTexts = texts.ToArray();

            _NewEntryIDs = new List<int>();
        }

        public override bool HandleInput(KeyEvent KeyEvent)
        {
            if (KeyEvent.KeyPressed && !Char.IsControl(KeyEvent.Unicode))
            {
                
            }
            else
            {
                switch (KeyEvent.Key)
                {
                    case Keys.Escape:
                    case Keys.Back:
                        CGraphics.FadeTo(EScreens.ScreenSong);
                        break;

                    case Keys.Enter:
                        CGraphics.FadeTo(EScreens.ScreenSong);
                        break;

                    case Keys.Down:
                        ChangePos(1);
                        break;

                    case Keys.Up:
                        ChangePos(-1);
                        break;

                    case Keys.Left:
                        ChangeRound(-1);
                        break;

                    case Keys.Right:
                        ChangeRound(1);
                        break;
                }
            }

            return true;
        }

        public override bool HandleMouse(MouseEvent MouseEvent)
        {
            if (MouseEvent.LB && IsMouseOver(MouseEvent))
            {
                
            }

            if (MouseEvent.LB)
            {
                CGraphics.FadeTo(EScreens.ScreenSong);
            }

            if (MouseEvent.RB)
            {
                CGraphics.FadeTo(EScreens.ScreenSong);
            }

            ChangePos(MouseEvent.Wheel);
            return true;
        }

        public override bool UpdateGame()
        {
            for (int p = 0; p < NumEntrys; p++)
            {
                if (_Pos + p < _Scores.Count)
                {
                    Texts[htTexts(TextNumber[p])].Visible = true;
                    Texts[htTexts(TextName[p])].Visible = true;
                    Texts[htTexts(TextScore[p])].Visible = true;
                    Texts[htTexts(TextDate[p])].Visible = true;

                    Texts[htTexts(TextNumber[p])].Text = (_Pos + p + 1).ToString();

                    string name = _Scores[_Pos + p].Name;
                    name += " [" + CLanguage.Translate(Enum.GetName(typeof(EGameDifficulty), _Scores[_Pos + p].Difficulty)) + "]";
                    if (_IsDuet)
                        name += " (P" + (_Scores[_Pos + p].LineNr + 1).ToString() + ")";
                    Texts[htTexts(TextName[p])].Text = name;

                    Texts[htTexts(TextScore[p])].Text = _Scores[_Pos + p].Score.ToString("00000");
                    Texts[htTexts(TextDate[p])].Text = _Scores[_Pos + p].Date;

                    if (isNewEntry(_Scores[_Pos + p].ID) == true)
                    {
                        Texts[htTexts(TextNumber[p])].Style = EStyle.BoldItalic;
                        Texts[htTexts(TextName[p])].Style = EStyle.BoldItalic;
                        Texts[htTexts(TextScore[p])].Style = EStyle.BoldItalic;
                        Texts[htTexts(TextDate[p])].Style = EStyle.BoldItalic;
                    }
                }
                else
                {
                    Texts[htTexts(TextNumber[p])].Visible = false;
                    Texts[htTexts(TextName[p])].Visible = false;
                    Texts[htTexts(TextScore[p])].Visible = false;
                    Texts[htTexts(TextDate[p])].Visible = false;
                }
            } 
            return true;
        }

        public override void OnShow()
        {
            base.OnShow();
            _Round = 0;
            _Pos = 0;
            _Scores = new List<SScores>();
            _NewEntryIDs.Clear();
            AddScoresToDB();
            LoadScores();

            UpdateGame();
        }

        public override bool Draw()
        {
            return base.Draw();
        }

        private bool isNewEntry(int id)
        {
            for(int i=0; i<_NewEntryIDs.Count; i++)
            {
                if (_NewEntryIDs[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddScoresToDB()
        {
            CPoints points = CGame.GetPoints();
            if (points == null)
                return;

            for (int round = 0; round < points.NumRounds; round++)
            {
                SPlayer[] player = points.GetPlayer(round, CGame.NumPlayer);

                for (int p = 0; p < player.Length; p++)
                {
                    if (player[p].Points > CSettings.MinScoreForDB && player[p].SongFinished && !CProfiles.IsGuestProfile(player[p].ProfileID))
                        _NewEntryIDs.Add(CDataBase.AddScore(player[p]));
                }
            }
        }

        private void LoadScores()
        {
            CPoints points = CGame.GetPoints();
            if (points == null)
                return;

            _Pos = 0;            
            for (int round = 0; round < points.NumRounds; round++)
            {
                SPlayer player = points.GetPlayer(round, CGame.NumPlayer)[0];
                CDataBase.LoadScore(ref _Scores, player);

                if (round == _Round)
                {
                    _IsDuet = false;

                    CSong song = CSongs.GetSong(player.SongID);
                    Texts[htTexts(TextSongName)].Text = song.Artist + " - " + song.Title;

                    switch (CGame.GameMode)
                    {
                        case EGameMode.Normal:
                            Texts[htTexts(TextSongMode)].Text = "TR_GAMEMODE_NORMAL";
                            break;

                        case EGameMode.Medley:
                            Texts[htTexts(TextSongMode)].Text = "TR_GAMEMODE_MEDLEY";
                            break;

                        case EGameMode.Duet:
                            Texts[htTexts(TextSongMode)].Text = "TR_GAMEMODE_DUET";
                            _IsDuet = true;
                            break;

                        default:
                            Texts[htTexts(TextSongMode)].Text = "TR_GAMEMODE_NORMAL";
                            break;
                    }
                }
            }
        }

        private void ChangePos(int Num)
        {
            if (Num > 0)
            {
                if (_Pos + Num + NumEntrys < _Scores.Count)
                    _Pos += Num;
            }
            if (Num < 0)
            {
                _Pos += Num;
                if (_Pos < 0)
                    _Pos = 0;
            }
        }

        private void ChangeRound(int Num)
        {
            CPoints points = CGame.GetPoints();
            if (points.NumRounds < 2)
                return;

            _Round += Num;

            if (Num > 0)
            {             
                if (_Round >= points.NumRounds)
                    _Round = 0;

                LoadScores();
            }

            if (Num < 0)
            {
                if (_Round < 0)
                    _Round = points.NumRounds - 1;

                LoadScores();
            }
        }
    }
}
