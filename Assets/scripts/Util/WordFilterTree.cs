using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



/// <summary>
/// 문자열 필터링을 위한 클래스
/// 트리구조로 구성되어 일치하는 문자열을 빠르게 찾아 바꾼다.
/// </summary>
public class WordFilterNode
{
    /// <summary>
    /// 한번에 스킵할 수 있는 총 문자 수
    /// </summary>
    static int maxSkipCount = 30;

    /// <summary>
    /// 필터링될 문자에 치환될 문자
    /// </summary>
    static char replaceChar = '*';

    /// <summary>
    /// 스킵되는 문자들 ('시123발' 같은 경우를 방지)
    /// </summary>
    static HashSet<char> skipChars = new HashSet<char>(new char[]{
        '1','2','3','4','5','6','7','8','9','0',
        '!','@','#','$','%','^','&','*','(',')',
        ' ',',','.','`','~','-','_','=','+','ㅡ',
        '\'','\\','\"',';',':'
    });



    bool endNode; // 문자열의 끝이라면 true. (하지만 자식이 있을 수 있다. ex> 시발, 시발새끼)

    Dictionary<char, WordFilterNode> childs; // 하위 노드들



    /// <summary>
    /// 비교할 문자열 등록
    /// </summary>
    /// <param name="_text"> 등록할 문자열 </param>
    /// <param name="_idx"> 진행 중인 문자열 인덱스.(기본값 0) </param>
    protected virtual void AddFilterText(string _text, int _idx = 0)
    {
        // 문자열의 끝이라면
        if (_text.Length <= _idx)
        {
            endNode = true;
            return;
        }

        char ch = _text[_idx];

        // 자식 노드 생성
        if (childs == null)
            childs = new Dictionary<char, WordFilterNode>();

        if (!childs.ContainsKey(ch))
            childs.Add(ch, new WordFilterNode());

        // 문자열을 자식에게 전달한다.
        childs[ch].AddFilterText(_text, _idx + 1);
    }



    /// <summary>
    /// 등록된 문자열들과 비교하여 일치하는 문자열을 replaceChar로 치환 한다.
    /// </summary>
    /// <param name="_text"> 비교할 문자 배열 </param>
    /// <param name="_idx"> 비교를 시작할 배열 인덱스 </param>
    /// <returns> 필터링이 되었다면 true, 아니라면 false </returns>
    protected bool Filtering(char[] _text, int _idx = 0)
    {
        // 하위 노드가 있다면 
        if (childs != null)
        {
            for (int i = 0; _idx + i < _text.Length; ++i)
            {
                char ch = _text[_idx + i];

                if (childs.ContainsKey(ch))                     // 하위 노드중 동일한 문자가 있다면
                {
                    if (childs[ch].Filtering(_text, _idx + i + 1))  // 일치하는 문자열을 찾았다면
                    {                                           
                        _text[_idx + i] = replaceChar;                  // 현재 글자를 필터링 한다.
                        return true;                                    // 성공을 반환한다.
                    }
                }
                else if (maxSkipCount <= i ||                   // 스킵 가능 최대 글자이거나
                         !IsSkipCharacter(ch))                  // 스킵되는 문자가 아니라면
                    break;                                          // 유효한 글자로 인정된다. (필터링하지 않는 글자)
            }
        }

        // 하위 노드가 없거나 다음 글자와 일치하는 하위 노드가 없다면
        return endNode;     // 현 노드의 endNode를 반환한다.
    }



    /// <summary>
    /// 스킵되는 문자인지를 확인한다.
    /// </summary>
    /// <param name="_ch"> 비교할 문자 </param>
    /// <returns> 스킵되는 문자라면 true, 아니라면 false </returns>
    public bool IsSkipCharacter(char _ch)
    {
        return skipChars.Contains(_ch);
    }
}





public class WordFilterTree : WordFilterNode
{
    /// <summary>
    /// 필터링되는 문자열의 최대 길이를 저장한다.
    /// </summary>
    int maxLength;


    public WordFilterTree() { }


    public WordFilterTree(string[] _texts)
    {
        AddFilterTexts(_texts);
    }


    /// <summary>
    /// 비교할 문자열들을 등록한다.
    /// </summary>
    /// <param name="_texts"></param>
    public void AddFilterTexts(string[] _texts)
    {
        for (int i = 0; i < _texts.Length; ++i)
            AddFilterText(_texts[i]);
    }


    /// <summary>
    /// 비교할 문자열을 등록한다.
    /// </summary>
    /// <param name="_text"> 등록할 문자열 </param>
    public void AddFilterText(string _text)
    {
        base.AddFilterText(_text);

        // 비교할 문자열의 최대 길이를 저장한다.
        maxLength = Math.Max(maxLength, _text.Length); 
    }



    /// <summary>
    /// 문자열을 비교하여 필터링한다.
    /// </summary>
    /// <param name="_text"></param>
    /// <returns></returns>
    public string Filtering(string _text)
    {
        // 조작이 용의하도록 char 배열로 변환
        char[] chs = _text.ToCharArray();

        for (int sIdx = 0; sIdx < chs.Length; ++sIdx)
            if (Filtering(chs, sIdx))                   // 필터링된 문자가 있다면
                sIdx = Math.Max(-1, sIdx - maxLength);      // 비교할 문자열의 최대 길이(==트리구조의 최고 깊이)만큼 다시 검사 한다.

        return new string(chs);
    }
}
