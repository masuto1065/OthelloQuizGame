using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OthelloBoard : MonoBehaviour
{
    public int CurrentTurn = 1;                     //現在のプレイヤー,0が白,1が黒
    public int BCountAcc = 0;                       //黒の問題正解数カウント用
    public int WCountAcc = 0;                       //白の問題正解数カウント用
    public int BCountTurn = 0;                      //黒のターン数カウント用
    public int WCountTurn = 0;                      //白のターン数カウント用
    public GameObject ScoreBoard;                   //結果表示用パネル
    public Text CorrectText;                        //「○」のテキスト
    public Text IncorrectText;                      //「×」のテキスト
    public Text ScoreBoardText;                     //結果表示用テキスト
    public GameObject Template;                     //セル作成用オブジェクト
    public int BoardSize = 8;                       //盤の大きさ
    public List<Color> PlayerChipColors;            //石の色
    public List<Vector2> DirectionList;             //Vector2(-1,1)は左上, Vector2(0,1)は上 ...
    static OthelloBoard instance;                   //インスタンス格納用
    public Text Turn;                               //ターン表示テキスト
    public Text TurnColor;                          //ターン表示テキストの色変更
    private const string BlackTurn = "黒のターン";  //現在のターン表示の定型文
    private const string WhiteTurn = "白のターン";
    public Text BScore;                             //途中スコア表示テキスト
    public Text WScore;
    public Text Question;                           //問題文用テキスト
    public Text Answer1, Answer2, Answer3, Answer4; //選択肢用テキスト
    public Text BAccRate;                           //正解率表示用テキスト
    public Text WAccRate;
    public AudioClip QuestionSE;                    //クイズ出題時のSE
    public AudioClip CorrectSE;                     //クイズ正解時のSE
    public AudioClip InCorrectSE;                   //クイズ不正解時のSE
    public AudioClip ResultSE;                      //最終結果表示時のSE
    AudioSource audioSource;
    public static OthelloBoard Instance { get { return instance; } }
    OthelloCell[,] OthelloCells;                   //8x8のOthelloCell.csの配列参照 (Start()で生成している)
    public int EnemyID { get { return (CurrentTurn + 1) % 2; } } //敵のIDは今のプレイヤーが0なら1,1なら0を返す

    private void Awake()
    {
        CorrectText = GameObject.Find("CorrectText").GetComponent<Text>();     //「○」表示用テキストをゲットコンポーネント
        IncorrectText = GameObject.Find("IncorrectText").GetComponent<Text>(); //「×」表示用テキストをゲットコンポーネント
        TurnColor = GameObject.Find("TurnText").GetComponent<Text>();          //現在のターン表示用テキストをゲットコンポーネント
    }

    void Start()
    {
        //オブジェクトに追加したAudioSourceを取得
        audioSource = GetComponent<AudioSource>();

        //○×テキストを非表示にする
        CorrectText.gameObject.SetActive(false);
        IncorrectText.gameObject.SetActive(false);

        //黒と白の正解率表示の初期値を代入
        BAccRate.text = "正解率:0%";
        WAccRate.text = "正解率:0%";

        //ターン表示テキストの初期値を代入
        Turn.text = BlackTurn;

        //石の枚数の途中スコア表示の初期値を代入
        BScore.text = "石の数:2";
        WScore.text = "石の数:2";

        instance = this;
        OthelloBoardIsSquareSize();

        //盤を作成
        OthelloCells = new OthelloCell[BoardSize, BoardSize];
        float cellAnchorSize = 1.0f / BoardSize;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                CreateNewCell(x, y, cellAnchorSize);
            }
        }
        GameObject.Destroy(Template);

        //盤の石の設定
        InitializeGame();

        //置くことができるマスの色を変える
        CanPlaceHereCellColor();
    }

    /* セル作成用関数                */
    /*(仮引数)      x:セルのx座標    */
    /*              y:セルのy座標    */
    /* cellAnchorSize:アンカーサイズ */
    private void CreateNewCell(int x, int y, float cellAnchorSize)
    {
        GameObject go = GameObject.Instantiate(Template, this.transform);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(x * cellAnchorSize, y * cellAnchorSize);
        r.anchorMax = new Vector2((x + 1) * cellAnchorSize, (y + 1) * cellAnchorSize);
        OthelloCell oc = go.GetComponent<OthelloCell>();
        OthelloCells[x, y] = oc;
        oc.Location.x = x;
        oc.Location.y = y;
    }

    /* 盤を調整する関数 */
    private void OthelloBoardIsSquareSize()
    {
        RectTransform rect = this.GetComponent<RectTransform>();
        if (Screen.width > Screen.height)  //画面サイズが幅 > 高さのとき
        {
            //幅を高さに合わせる
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.height);
        }
        else                               //画面サイズが幅 < 高さのとき
            //高さを幅に合わせる
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.width);
    }
    /* 盤面の初期配置をする関数 */
    public void InitializeGame()
    {
        ScoreBoard.gameObject.SetActive(false);  //結果表示用パネルを非表示にする
        for (int y = 0; y < BoardSize; y++)      //すべてを空マスに設定
        {
            for (int x = 0; x < BoardSize; x++)
            {
                OthelloCells[x, y].OwnerID = -1; //空
            }
        }
        OthelloCells[3, 3].OwnerID = 1;          //白
        OthelloCells[4, 4].OwnerID = 1;          //白
        OthelloCells[4, 3].OwnerID = 0;          //黒
        OthelloCells[3, 4].OwnerID = 0;          //黒
    }

    /* クリックしたマスに置けるか判定する関数         */
    /* (仮引数) location:クリックしたマスの座標       */
    /* (戻り値) true:選択したマスに置くことができる   */
    /*         false:選択したマスに置くことができない */
    internal bool CanPlaceHere(Vector2 location)
    {
        if (OthelloCells[(int)location.x, (int)location.y].OwnerID != -1)          //選択したマスが-1(空)でなければ(マスが開いてるか)
            return false;                                                          //選択していたマスが空でないなら失敗
        for (int direction = 0; direction < DirectionList.Count; direction++)      //全方向に対して挟める方角があるかを判定
        {
            Vector2 directionVector = DirectionList[direction];
            if (FindAllyChipOnOtherSide(directionVector, location, false) != null) //指定された方向に対して挟む自分の石が存在しているか？
            {
                return true;                                                       //一つの方向でも見つければそれで終わり
            }
        }
        return false;                                                              //挟める方角が無ければ失敗
    }

    /* 選択したマスに石を置く関数        */
    /* (仮引数) othelloCell:選択したマス */
    internal void PlaceHere(OthelloCell othelloCell)
    {
        //全方向に対して挟むことができるか調べる
        for (int direction = 0; direction < DirectionList.Count; direction++)
        {
            Vector2 directionVector = DirectionList[direction];
            //指定された方向に対して挟む自分の石が存在しているか？
            OthelloCell onOtherSide = FindAllyChipOnOtherSide(directionVector, othelloCell.Location, false);
            //もし、その方向に挟むことができる自分の石が存在しているなら
            if (onOtherSide != null)
            {
                //挟んだ敵の石をひっくり返す
                ChangeOwnerBetween(othelloCell, onOtherSide, directionVector);
            }
        }

        //選択したマスに自分の石を置く
        OthelloCells[(int)othelloCell.Location.x, (int)othelloCell.Location.y].OwnerID = CurrentTurn;

        //正解数をカウントする
        if (CurrentTurn == 0)      //白
        {
            WCountAcc++;
        }
        else if (CurrentTurn == 1) //黒
        {
            BCountAcc++;
        }

        //クイズ正解時のSEを鳴らす
        audioSource.PlayOneShot(CorrectSE);
        //「○」を表示する
        CorrectText.gameObject.SetActive(true);
    }

    /* 指定された方角に対して、挟むことができるかを判定する再帰関数                          */
    /* (仮引数) directionVector:2次元ベクトル                                                */
    /*                     from:選択したマスの座標                                           */
    /*               EnemyFound:挟めるマスをみつけたかの状態                                 */
    /* (戻り値) null:挟める石を見つけられなかった                                            */
    /* OthelloCells[(int)to.x, (int)to.y]:最後に見つけた挟むことができる相手の石の座標を返す */
    private OthelloCell FindAllyChipOnOtherSide(Vector2 directionVector, Vector2 from, bool EnemyFound)
    {
        Vector2 to = from + directionVector;
        //ボードの外に出てないか、空マスでないか
        if (IsInRangeOfBoard(to) && OthelloCells[(int)to.x, (int)to.y].OwnerID != -1) 
        {
            //見つかったマスのオセロは自分のオセロか
            if (OthelloCells[(int)to.x, (int)to.y].OwnerID == OthelloBoard.Instance.CurrentTurn)
            {
                //既に間に一回敵のオセロを見つけているか(つまり挟んだか)
                if (EnemyFound)
                    //最後に見つけた挟むことができる相手の石の座標を返す
                    return OthelloCells[(int)to.x, (int)to.y]; 
                return null;
            }
            else
                //見つかったのは敵の石なので、自分の石を見つけるまでこの関数を繰り返す(その一方向に向かって)
                return FindAllyChipOnOtherSide(directionVector, to, true);
        }
        //ここまでにreturnされない場合nullを返す
        return null; 
    }

    /* 指定したマスの座標を返す                            */
    /* (仮引数) point:マスの座標                           */
    /* (戻り値) 盤サイズ内の座標だったとき、その座標を返す */
    private bool IsInRangeOfBoard(Vector2 point)
    {
        return point.x >= 0 && point.x < BoardSize && point.y >= 0 && point.y < BoardSize;
    }

    /* fromからtoの間の挟んだ敵のセルをひっくり返す関数                   */
    /* (仮引数) 　from:クリックしたマスの座標                             */
    /*              to:指定された方向の挟むためのもう一つの自分の石の座標 */
    /* directionVector:2次元ベクトル                                      */
    private void ChangeOwnerBetween(OthelloCell from, OthelloCell to, Vector2 directionVector)
    {
        for (Vector2 location = from.Location + directionVector; location != to.Location; location += directionVector)
        {
            OthelloCells[(int)location.x, (int)location.y].OwnerID = CurrentTurn;
        }
    }

    /* ターン終了後の処理をする関数                  */
    /* (仮引数) isAlreadyEnded:falseを与えて呼び出す */
    internal void EndTurn(bool isAlreadyEnded) 
    {
        //「○」が表示されていなければ
        if(CorrectText.gameObject.activeSelf == false)
        {
            //クイズ不正解時のSEを鳴らす
            audioSource.PlayOneShot(InCorrectSE);
            //「×」を表示する
            IncorrectText.gameObject.SetActive(true);
        }

        //クイズの正解率を計算 (正解数/自分のターン数)×100
        if(CurrentTurn == 0)      //白
        {
            WCountTurn++;
            WAccRate.text = "正解率:" + (int)(((double)WCountAcc / (double)WCountTurn) * 100) + "%";
        }
        else if(CurrentTurn == 1) //黒
        {
            BCountTurn++;
            BAccRate.text = "正解率:" + (int)(((double)BCountAcc / (double)BCountTurn) * 100) + "%";
        }

        //プレイヤーを入れ替える
        CurrentTurn = EnemyID;
        
        //ターン表示も入れ替える
        if (CurrentTurn == 0) //白
        {
            Turn.text = WhiteTurn;
            TurnColor.color = new Color(1.0f,1.0f,1.0f,1.0f);
        }
        else                  //黒
        {
            Turn.text = BlackTurn;
            TurnColor.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        }

        //それぞれの石の数を数える
        int WhiteScore = CountScoreFor(0); //白
        int BlackScore = CountScoreFor(1); //黒

        //石の枚数の途中スコア表示を代入
        WScore.text = "石の数:" + WhiteScore; //白
        BScore.text = "石の数:" + BlackScore; //黒

        //マスの通常時の色を元に戻した後、再び置くことができるマスの色を変える
        FixCellColor();
        CanPlaceHereCellColor();

        //次のプレイヤーが置けるマスがあるか確認する
        for (int y = 0; y < BoardSize; y++) 
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (CanPlaceHere(new Vector2(x, y)))
                {
                    //あったので次のプレイヤーのターンへ
                    return; 
                }
            }
        }

        //次のプレイヤーが置けるマスがなかった場合
        if (isAlreadyEnded)
            //ゲーム終了
            GameOver(); 
        else
        {
            //誤って消費してないはずの1ターン数を加算しないために1減らした上でEndTurnを呼ぶ
            if (CurrentTurn == 0)      //白
            {
                WCountTurn--;
            }
            else if (CurrentTurn == 1) //黒
            {
                BCountTurn--;
            }

            //もう一度同じ処理をする。ただしisALreadyEndedフラグを立てる もう一方が置けるマスがあるか
            EndTurn(true); 
        }
    }

    /* ゲーム終了後の処理をする関数 */
    public void GameOver()
    {
        //マスを無効にする
        BoardActiveFalse();

        //「○」を表示する
        CorrectText.gameObject.SetActive(true);

        //それぞれの石の数を数える
        int white = CountScoreFor(0); //白
        int black = CountScoreFor(1); //黒
        //ぞれぞれの正解率を計算する (正解数/自分のターン数)×100 
        int WhiteAccRate = (int)(((double)WCountAcc / (double)WCountTurn) * 100); //白
        int BlackAccRate = (int)(((double)BCountAcc / (double)BCountTurn) * 100); //黒
        //最終的なスコアを計算する 石の数+正解率
        int ScoreWhite = white + (int)(((double)WCountAcc / (double)WCountTurn) * 100); //白
        int ScoreBlack = black + (int)(((double)BCountAcc / (double)BCountTurn) * 100); //黒

        //結果の表示
        if (ScoreWhite > ScoreBlack)
            ScoreBoardText.text = "  白の勝利！" + "\n黒:" + black + "枚 + " + BlackAccRate + "% = " + ScoreBlack
                + "\n白:" + white + "枚 + " + WhiteAccRate + "% = " + ScoreWhite;
        else if (ScoreBlack > ScoreWhite)
            ScoreBoardText.text = "  黒の勝利！" + "\n黒:" + black + "枚 + " + BlackAccRate + "% = " + ScoreBlack
                + "\n白:" + white + "枚 + " + WhiteAccRate + "% = " + ScoreWhite;
        else
            ScoreBoardText.text = "  引き分け！" + "\n黒:" + black + "枚 + " + BlackAccRate + "% = " + ScoreBlack
                + "\n白:" + white + "枚 + " + WhiteAccRate + "% = " + ScoreWhite;
        //最終結果表示
        audioSource.PlayOneShot(ResultSE);
        ScoreBoard.gameObject.SetActive(true);
    }

    /* 石の数を数える関数         */
    /* (仮引数) owner:0が白,1が黒 */
    /* (戻り値) count:石の数      */
    private int CountScoreFor(int owner)
    {
        int count = 0;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (OthelloCells[x, y].OwnerID == owner)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /* マスを有効にする関数 */
    public void BoardActiveTrue()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if(OthelloCells[x, y].OwnerID == -1)
                {
                    OthelloCells[x, y].GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    /* マスを無効にする関数 */
    public void BoardActiveFalse()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                OthelloCells[x, y].GetComponent<Button>().interactable = false;
            }
        }
        //○×のテキストを非表示にする
        CorrectText.gameObject.SetActive(false);
        IncorrectText.gameObject.SetActive(false);
    }

    /* 選択可能なマスの通常時の色を返る関数 */
    public void CanPlaceHereCellColor()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if(CanPlaceHere(new Vector2(x, y)))
                {
                    Button b = OthelloCells[x, y].GetComponent<Button>();
                    ColorBlock cb = b.colors;
                    cb.normalColor = new Color((88f/255f), (157f/255f), (82f/255f), 1.0f);
                    b.colors = cb;
                }

                //既に石が置かれているマスを無効化
                /*
                if(OthelloCells[x, y].OwnerID != -1)
                {
                    OthelloCells[x, y].GetComponent<Button>().interactable = false;
                }
                */
            }
        }
    }

    /* 通常時のマスの色を元に戻す関数 */
    public void FixCellColor()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                Button b = OthelloCells[x, y].GetComponent<Button>();
                ColorBlock cb = b.colors;
                cb.normalColor = new Color((29f / 255f), (106f / 255f), (22f / 255f), 1.0f);
                b.colors = cb;
            }
        }
    }

    //クイズ出題用関数
    public void OthelloQuetion(OthelloCell othelloCell)
    {
        //クイズ出題時のSEを鳴らす
        audioSource.PlayOneShot(QuestionSE);

        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "カバの汗の色はどれ？";
            Answer1.text = "赤"; Answer2.text = "青"; Answer3.text = "緑"; Answer4.text = "紫";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "「まぐろ」の漢字はどれ？";
            Answer1.text = "鰯"; Answer2.text = "鮪"; Answer3.text = "鯛"; Answer4.text = "鰻";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "キリンの一日の睡眠時間はどれ？";
            Answer1.text = "約8時間"; Answer2.text = "約20時間"; Answer3.text = "約20分"; Answer4.text = "約12時間";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 3))
        {
            Question.text = "「馬大頭」の読み方はどれ？";
            Answer1.text = "オニヤンマ"; Answer2.text = "キリギリス"; Answer3.text = "スズメバチ"; Answer4.text = "カブトムシ";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 4))
        {
            Question.text = "88×23の答えはどれ？";
            Answer1.text = "1867"; Answer2.text = "1904"; Answer3.text = "2004"; Answer4.text = "2024";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "空手の発祥の地はどれ？";
            Answer1.text = "北海道"; Answer2.text = "沖縄県"; Answer3.text = "新潟県"; Answer4.text = "鹿児島県";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "日本発祥の食べ物はどれ？";
            Answer1.text = "カルボナーラ"; Answer2.text = "ミネストローネ"; Answer3.text = "ナポリタン"; Answer4.text = "ティラミス";
        }
        if (((int)othelloCell.Location.x == 0) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "世界三大美女の一人はどれ？";
            Answer1.text = "卑弥呼"; Answer2.text = "紫式部"; Answer3.text = "清少納言"; Answer4.text = "小野小町";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "「心理学」の英語はどれ？";
            Answer1.text = "Psychology"; Answer2.text = "Physics"; Answer3.text = "Physiology"; Answer4.text = "Pathology";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "日本で一番面積の大きい県はどれ？";
            Answer1.text = "新潟県"; Answer2.text = "長野県"; Answer3.text = "福島県"; Answer4.text = "岩手県";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "36×40÷24の答えはどれ？";
            Answer1.text = "44"; Answer2.text = "53"; Answer3.text = "60"; Answer4.text = "78";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 3))
        {
            Question.text = "日本で一番長い川はどれ？";
            Answer1.text = "石狩川"; Answer2.text = "信濃川"; Answer3.text = "最上川"; Answer4.text = "木曽川";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 4))
        {
            Question.text = "日本の国鳥はどれ？";
            Answer1.text = "キジ"; Answer2.text = "カラス"; Answer3.text = "キツツキ"; Answer4.text = "トキ";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "アメリカ合衆国の州の数はどれ？";
            Answer1.text = "40"; Answer2.text = "45"; Answer3.text = "50"; Answer4.text = "55";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "日本の初代内閣総理大臣はどれ？";
            Answer1.text = "伊藤博文"; Answer2.text = "黒田清隆"; Answer3.text = "菅義偉"; Answer4.text = "岸田文雄";
        }
        if (((int)othelloCell.Location.x == 1) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "111×111の答えはどれ？";
            Answer1.text = "11211"; Answer2.text = "12321"; Answer3.text = "13231"; Answer4.text = "13431";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "(23+18)×12の答えはどれ？";
            Answer1.text = "392"; Answer2.text = "492"; Answer3.text = "574"; Answer4.text = "635";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "富士山の標高はどれ？";
            Answer1.text = "356m"; Answer2.text = "895m"; Answer3.text = "2675m"; Answer4.text = "3776m";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "日本国旗に使われている色はどれ？";
            Answer1.text = "赤"; Answer2.text = "緑"; Answer3.text = "紫"; Answer4.text = "青";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 3))
        {
            Question.text = "黄色い食べ物はどれ？";
            Answer1.text = "バナナ"; Answer2.text = "リンゴ"; Answer3.text = "イチゴ"; Answer4.text = "ブドウ";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 4))
        {
            Question.text = "一番寒い季節はどれ？";
            Answer1.text = "春"; Answer2.text = "夏"; Answer3.text = "秋"; Answer4.text = "冬";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "2×3×23の答えはどれ？";
            Answer1.text = "53"; Answer2.text = "96"; Answer3.text = "138"; Answer4.text = "233";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "世界最小面積の国はどれ？";
            Answer1.text = "バチカン"; Answer2.text = "モナコ"; Answer3.text = "ナウル"; Answer4.text = "ツバル";
        }
        if (((int)othelloCell.Location.x == 2) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "「あらためる」の送り仮名はどれ？";
            Answer1.text = "改らためる"; Answer2.text = "改ためる"; Answer3.text = "改める"; Answer4.text = "改る";
        }
        if (((int)othelloCell.Location.x == 3) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "「イカ」の漢字はどれ？";
            Answer1.text = "海豹"; Answer2.text = "海星"; Answer3.text = "烏賊"; Answer4.text = "蛤";
        }
        if (((int)othelloCell.Location.x == 3) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "新潟という漢字の総画数はどれ？";
            Answer1.text = "27"; Answer2.text = "28"; Answer3.text = "29"; Answer4.text = "30";
        }
        if (((int)othelloCell.Location.x == 3) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "19+27の答えはどれ？";
            Answer1.text = "30"; Answer2.text = "46"; Answer3.text = "47"; Answer4.text = "56";
        }
        if (((int)othelloCell.Location.x == 3) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "「たしかめる」の送り仮名はどれ？";
            Answer1.text = "確しかめる"; Answer2.text = "確かめる"; Answer3.text = "確める"; Answer4.text = "確る";
        }
        if (((int)othelloCell.Location.x == 3) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "土星の英語はどれ？";
            Answer1.text = "Mars"; Answer2.text = "Mercury"; Answer3.text = "Venus"; Answer4.text = "Saturn";
        }
        if (((int)othelloCell.Location.x == 3) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "日本最小面積の県はどれ？";
            Answer1.text = "沖縄県"; Answer2.text = "香川県"; Answer3.text = "神奈川県"; Answer4.text = "佐賀県";
        }
        if (((int)othelloCell.Location.x == 4) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "日本人で一番多い血液型はどれ？";
            Answer1.text = "A型"; Answer2.text = "B型"; Answer3.text = "O型"; Answer4.text = "AB型";
        }
        if (((int)othelloCell.Location.x == 4) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "スカイツリーの高さはどれ？";
            Answer1.text = "417m"; Answer2.text = "596m"; Answer3.text = "634m"; Answer4.text = "763m";
        }
        if (((int)othelloCell.Location.x == 4) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "メ～と鳴く動物はどれ？";
            Answer1.text = "カラス"; Answer2.text = "牛"; Answer3.text = "ヒツジ"; Answer4.text = "人間";
        }
        if (((int)othelloCell.Location.x == 4) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "太陽が昇ってくる方角はどれ？";
            Answer1.text = "東"; Answer2.text = "西"; Answer3.text = "南"; Answer4.text = "北";
        }
        if (((int)othelloCell.Location.x == 4) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "ドイツの首都はどれ？";
            Answer1.text = "ミュンヘン"; Answer2.text = "ハンブルク"; Answer3.text = "ケルン"; Answer4.text = "ベルリン";
        }
        if (((int)othelloCell.Location.x == 4) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "正しい象の歯の本数はどれ？";
            Answer1.text = "4本"; Answer2.text = "16本"; Answer3.text = "32本"; Answer4.text = "64本";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "四国でない県はどれ？";
            Answer1.text = "愛媛県"; Answer2.text = "香川県"; Answer3.text = "高知県"; Answer4.text = "山口県";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "木曜日の英語はどれ？";
            Answer1.text = "Thursday"; Answer2.text = "Saturday"; Answer3.text = "Monday"; Answer4.text = "Tuesday";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "1分を秒数で表したのはどれ？";
            Answer1.text = "30秒"; Answer2.text = "60秒"; Answer3.text = "70秒"; Answer4.text = "100秒";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 3))
        {
            Question.text = "日本の都道府県の数はどれ？";
            Answer1.text = "37"; Answer2.text = "41"; Answer3.text = "47"; Answer4.text = "50";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 4))
        {
            Question.text = "日本の首都はどれ？";
            Answer1.text = "新潟県"; Answer2.text = "京都府"; Answer3.text = "北海道"; Answer4.text = "東京都";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "バスケットボールの漫画はどれ？";
            Answer1.text = "イナズマイレブン"; Answer2.text = "ブルーロック"; Answer3.text = "鬼滅の刃"; Answer4.text = "スラムダンク";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "水に溶けやすい気体はどれ？";
            Answer1.text = "酸素"; Answer2.text = "アンモニア"; Answer3.text = "水素"; Answer4.text = "窒素";
        }
        if (((int)othelloCell.Location.x == 5) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "母の日に母に贈る花はどれ？";
            Answer1.text = "バラ"; Answer2.text = "コスモス"; Answer3.text = "アサガオ"; Answer4.text = "カーネーション";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "「三四郎」の作者はどれ？";
            Answer1.text = "川端康成"; Answer2.text = "夏目漱石"; Answer3.text = "宮沢賢治"; Answer4.text = "太宰治";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "電気を通すのはどれ？";
            Answer1.text = "紙"; Answer2.text = "アルミホイル"; Answer3.text = "ガラス"; Answer4.text = "ダイヤモンド";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "64÷4×27の答えはどれ？";
            Answer1.text = "178"; Answer2.text = "285"; Answer3.text = "383"; Answer4.text = "432";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 3))
        {
            Question.text = "Julyの日本語はどれ？";
            Answer1.text = "2月"; Answer2.text = "6月"; Answer3.text = "7月"; Answer4.text = "9月";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 4))
        {
            Question.text = "本能寺の変が起きた年はどれ？";
            Answer1.text = "1563年"; Answer2.text = "1582年"; Answer3.text = "1600年"; Answer4.text = "1643年";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "芥川龍之介の作品はどれ？";
            Answer1.text = "羅生門"; Answer2.text = "走れメロス"; Answer3.text = "山月記"; Answer4.text = "銀河鉄道の夜";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "太陽の直径はどれ？";
            Answer1.text = "約1.39万km"; Answer2.text = "約13.9万km"; Answer3.text = "約139万km"; Answer4.text = "約1390万km";
        }
        if (((int)othelloCell.Location.x == 6) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "桃太郎のお供だった鳥はどれ？";
            Answer1.text = "タカ"; Answer2.text = "ツバメ"; Answer3.text = "キジ"; Answer4.text = "ハヤブサ";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 0))
        {
            Question.text = "日本の在来種でない動物はどれ？";
            Answer1.text = "ムササビ"; Answer2.text = "ヤンバルクイナ"; Answer3.text = "アライグマ"; Answer4.text = "アオダイショウ";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 1))
        {
            Question.text = "結果が544となるのはどれ？";
            Answer1.text = "17×32"; Answer2.text = "72×7"; Answer3.text = "259＋255"; Answer4.text = "643－95";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 2))
        {
            Question.text = "関ヶ原の戦いが起きた年はどれ？";
            Answer1.text = "1500年"; Answer2.text = "1600年"; Answer3.text = "1700年"; Answer4.text = "1800年";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 3))
        {
            Question.text = "「口車に乗る」の意味はどれ？";
            Answer1.text = "仲良くなる"; Answer2.text = "協力する"; Answer3.text = "騙される"; Answer4.text = "褒められる";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 4))
        {
            Question.text = "JFAの意味はどれ？";
            Answer1.text = "日本野球機構"; Answer2.text = "日本卓球協会"; Answer3.text = "日本水泳連盟"; Answer4.text = "日本サッカー協会";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 5))
        {
            Question.text = "88歳を表すのはどれ？";
            Answer1.text = "還暦"; Answer2.text = "喜寿"; Answer3.text = "傘寿"; Answer4.text = "米寿";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 6))
        {
            Question.text = "現存する世界最古の国はどれ？";
            Answer1.text = "日本"; Answer2.text = "ギリシャ"; Answer3.text = "エジプト"; Answer4.text = "中国";
        }
        if (((int)othelloCell.Location.x == 7) && ((int)othelloCell.Location.y == 7))
        {
            Question.text = "世界遺産でないのはどれ？";
            Answer1.text = "屋久島"; Answer2.text = "佐渡島の金山"; Answer3.text = "姫路城"; Answer4.text = "厳島神社";
        }
    }
}