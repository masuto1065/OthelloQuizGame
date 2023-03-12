using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OthelloCell : MonoBehaviour
{
    int ownerID = -1;
    public Image ChipImage;
    public Vector2 Location;       //座標[x,y]
    public Text CellEffectText;
    static int PlayerAns = -1;     //解答が正解だった場合：1,　不正解だった場合：0を入れる変数
    static int PressedX, PressedY; //選択したマスの座標を保存する変数
    public int OwnerID
    {
        get { return ownerID; }
        set
        {
            ownerID = value;
            ChipImage.color = OthelloBoard.Instance.PlayerChipColors[ownerID + 1];
            if (ownerID == -1)                                    //マスの値が-1(空)なら
                this.GetComponent<Button>().interactable = true;  //ボタンを押せる
            else                                                  //それ以外なら
                this.GetComponent<Button>().interactable = false; //ボタンを押せない
        }
    }
    //プレイヤーが選択したマスに石を置くことができるならば、問題を表示する
    public void CellPressed()
    {
        //もし、選択したマスに石を置くことができるならば
        if (OthelloBoard.Instance.CanPlaceHere(this.Location))
        {
            //セルを無効にする
            OthelloBoard.Instance.BoardActiveFalse();
            //選択したマスの問題を表示
            OthelloBoard.Instance.OthelloQuetion(this);
            //選択したマスの座標を保存
            PressedX = (int)this.Location.x;
            PressedY = (int)this.Location.y;
        }
    }

    //1番目のボタンを選択した時の処理
    public void QuestionAnswer1()
    {
        //CellPressedで保存したマスの座標を読み込む
        this.Location.x = PressedX;
        this.Location.y = PressedY;
        //選択したマスに石を置くことができるならば
        if (OthelloBoard.Instance.CanPlaceHere(this.Location))
        {
            //1番目のボタンを押したときの選択していたマスが以下のようならば
            if (((PressedX == 0) && (PressedY == 0)) ||
                ((PressedX == 0) && (PressedY == 3)) ||
                ((PressedX == 1) && (PressedY == 0)) ||
                ((PressedX == 1) && (PressedY == 4)) ||
                ((PressedX == 1) && (PressedY == 6)) ||
                ((PressedX == 2) && (PressedY == 2)) ||
                ((PressedX == 2) && (PressedY == 3)) ||
                ((PressedX == 2) && (PressedY == 6)) ||
                ((PressedX == 4) && (PressedY == 0)) ||
                ((PressedX == 4) && (PressedY == 5)) ||
                ((PressedX == 4) && (PressedY == 7)) ||
                ((PressedX == 5) && (PressedY == 1)) ||
                ((PressedX == 6) && (PressedY == 5)) ||
                ((PressedX == 7) && (PressedY == 1)) ||
                ((PressedX == 7) && (PressedY == 6)))
            {
                //PlayerAns = 1(正解)
                PlayerAns = 1;
            }
            else
            {
                //PlayerAns = 0(不正解)
                PlayerAns = 0;
            }
            //もし、問題に正解しているならば
            if (PlayerAns == 1)
            {
                //選択していたマスに石を置く
                OthelloBoard.Instance.PlaceHere(this);
            }
            //プレイヤー交代
            OthelloBoard.Instance.EndTurn(false);
            PlayerAns = -1;
            //セルを有効にする
            OthelloBoard.Instance.BoardActiveTrue();
        }
    }

    //2番目のボタンを選択した時の処理
    public void QuestionAnswer2()
    {
        //CellPressedで保存したマスの座標を読み込む
        this.Location.x = PressedX;
        this.Location.y = PressedY;
        //選択したマスに石を置くことができるならば
        if (OthelloBoard.Instance.CanPlaceHere(this.Location))
        {
            //2番目のボタンを押したときの選択していたマスが以下のようならば
            if (((PressedX == 0) && (PressedY == 1)) ||
                ((PressedX == 0) && (PressedY == 5)) ||
                ((PressedX == 1) && (PressedY == 3)) ||
                ((PressedX == 1) && (PressedY == 7)) ||
                ((PressedX == 2) && (PressedY == 0)) ||
                ((PressedX == 3) && (PressedY == 1)) ||
                ((PressedX == 3) && (PressedY == 2)) ||
                ((PressedX == 3) && (PressedY == 5)) ||
                ((PressedX == 3) && (PressedY == 7)) ||
                ((PressedX == 5) && (PressedY == 2)) ||
                ((PressedX == 5) && (PressedY == 6)) ||
                ((PressedX == 6) && (PressedY == 0)) ||
                ((PressedX == 6) && (PressedY == 1)) ||
                ((PressedX == 6) && (PressedY == 4)) ||
                ((PressedX == 7) && (PressedY == 2)) ||
                ((PressedX == 7) && (PressedY == 7)))
            {
                //PlayerAns = 1(正解)
                PlayerAns = 1;
            }
            else
            {
                //PlayerAns = 0(不正解)
                PlayerAns = 0;
            }
            //もし、問題に正解しているならば
            if (PlayerAns == 1)
            {
                //選択していたマスに石を置く
                OthelloBoard.Instance.PlaceHere(this);
            }
            //プレイヤー交代
            OthelloBoard.Instance.EndTurn(false);
            PlayerAns = -1;
            //セルを有効にする
            OthelloBoard.Instance.BoardActiveTrue();
        }
    }
    //3番目のボタンを選択した時の処理
    public void QuestionAnswer3()
    {
        //CellPressedで保存したマスの座標を読み込む
        this.Location.x = PressedX;
        this.Location.y = PressedY;
        //選択したマスに石を置くことができるならば
        if (OthelloBoard.Instance.CanPlaceHere(this.Location))
        {
            //3番目のボタンを押したときの選択していたマスが以下のようならば
            if (((PressedX == 0) && (PressedY == 2)) ||
                ((PressedX == 0) && (PressedY == 6)) ||
                ((PressedX == 1) && (PressedY == 2)) ||
                ((PressedX == 1) && (PressedY == 5)) ||
                ((PressedX == 2) && (PressedY == 5)) ||
                ((PressedX == 2) && (PressedY == 7)) ||
                ((PressedX == 3) && (PressedY == 0)) ||
                ((PressedX == 4) && (PressedY == 1)) ||
                ((PressedX == 4) && (PressedY == 2)) ||
                ((PressedX == 5) && (PressedY == 3)) ||
                ((PressedX == 6) && (PressedY == 3)) ||
                ((PressedX == 6) && (PressedY == 6)) ||
                ((PressedX == 6) && (PressedY == 7)) ||
                ((PressedX == 7) && (PressedY == 0)) ||
                ((PressedX == 7) && (PressedY == 3)))
            {
                //PlayerAns = 1(正解)
                PlayerAns = 1;
            }
            else
            {
                //PlayerAns = 0(不正解)
                PlayerAns = 0;
            }
            //もし、問題に正解しているならば
            if (PlayerAns == 1)
            {
                //選択していたマスに石を置く
                OthelloBoard.Instance.PlaceHere(this);
            }
            //プレイヤー交代
            OthelloBoard.Instance.EndTurn(false);
            PlayerAns = -1;
            //セルを有効にする
            OthelloBoard.Instance.BoardActiveTrue();
        }
    }

    //4番目のボタンを選択した時の処理
    public void QuestionAnswer4()
    {
        //CellPressedで保存したマスの座標を読み込む
        this.Location.x = PressedX;
        this.Location.y = PressedY;
        //選択したマスに石を置くことができるならば
        if (OthelloBoard.Instance.CanPlaceHere(this.Location))
        {
            //4番目のボタンを押したときの選択していたマスが以下のようならば
            if (((PressedX == 0) && (PressedY == 4)) ||
                ((PressedX == 0) && (PressedY == 7)) ||
                ((PressedX == 1) && (PressedY == 1)) ||
                ((PressedX == 2) && (PressedY == 1)) ||
                ((PressedX == 2) && (PressedY == 4)) ||
                ((PressedX == 3) && (PressedY == 6)) ||
                ((PressedX == 4) && (PressedY == 6)) ||
                ((PressedX == 5) && (PressedY == 0)) ||
                ((PressedX == 5) && (PressedY == 4)) ||
                ((PressedX == 5) && (PressedY == 5)) ||
                ((PressedX == 5) && (PressedY == 7)) ||
                ((PressedX == 6) && (PressedY == 2)) ||
                ((PressedX == 7) && (PressedY == 4)) ||
                ((PressedX == 7) && (PressedY == 5)))
            {
                //PlayerAns = 1(正解)
                PlayerAns = 1;
            }
            else
            {
                //PlayerAns = 0(不正解)
                PlayerAns = 0;
            }
            //もし、問題に正解しているならば
            if (PlayerAns == 1)
            {
                //選択していたマスに石を置く
                OthelloBoard.Instance.PlaceHere(this);
            }
            //プレイヤー交代
            OthelloBoard.Instance.EndTurn(false);
            PlayerAns = -1;
            //セルを有効にする
            OthelloBoard.Instance.BoardActiveTrue();
        }
    }
}