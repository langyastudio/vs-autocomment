using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace HKFY.AutoComment2015
{
     public struct ITEMDATA
     {
        public  int     itemType;
        public string  itemText;       
     }   

    /// <summary>
    /// 针对注释定义相应的响应函数
    /// </summary>
    class CommentFun
    {
        TextDocument _textDoc = null; //当前激活的文档
        string _Author = Environment.UserName + " ";
        string _FuncSeparater = "//================================================================================";
        int _FuncSeparaterNum = 0;
        string _ChangeHistory = "修改历史";
        List<ITEMDATA> _itemList = null;

        public CommentFun()
        {
            Initialize();

            _FuncSeparaterNum = _FuncSeparater.Length - 2;
            _itemList = new List<ITEMDATA>(4);
        }

        private void Initialize()
        {
            DTE myDTE;
            TextDocument txtDoc;
            myDTE = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;
            if (null == myDTE)
                return;

            txtDoc = myDTE.ActiveDocument.Object("TextDocument") as TextDocument;
            if (null == txtDoc)
                return;

            _textDoc = txtDoc;
        }      

        public void ChangeDetailCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            int iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            int iLineLength = 0;
            String strFunText;

            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength);

            string strSpace = "";
            for (int iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            outText.Insert(strSpace + "// 修改说明:<详细说明修改原因及内容>" + "\n");
            outText.Insert(strSpace + "// 修改人: " + _Author + DateTime.Now.ToString("yyyy-MM-dd") + "\n");

            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        private bool ParseFunctionText(string funText)
        {
            if (funText == null)
                return (false);

            string strItem;
            ITEMDATA idata;
            string[] strSplit = funText.Split(new char[] { '(' });
            if (strSplit == null || strSplit.Length == 1)
                return (false);

            //1.0 解析函数名称部分
            if (strSplit.Length > 2)
                strItem = strSplit[strSplit.Length - 2].Trim();
            else
                strItem = strSplit[0].Trim();
            //当返回值和函数名用Tab分开,用空格切割不能正确的得到返回值,用空格替换Tab
            strItem.Replace('\t', ' ');
            string[] strHeadSplit = strItem.Split(new char[] { ' ' });
            strItem = strHeadSplit[strHeadSplit.Length - 1].Trim();
            idata = new ITEMDATA();
            idata.itemType = 1;
            idata.itemText = strItem.Trim();
            if (idata.itemText.Length > 0)
            {
                if (idata.itemText[0] == '*' || idata.itemText[0] == '&') //去掉函数前的* 和&
                    idata.itemText = idata.itemText.Remove(0, 1);
            }
            _itemList.Add(idata);

            //2.0 解析参数部分
            strItem = strSplit[strSplit.Length - 1].Trim();
            if (strItem.Substring(0, 1) != ")")//此时有参数
            {
                int iend = strItem.IndexOf(")", 0);
                string strParams = strItem.Substring(0, iend).Trim();
                string[] strParamSplit = strParams.Split(new char[] { ',' });
                foreach (string strTemp in strParamSplit)
                {
                    int strIndex = 0;
                    bool haveEqel = false;
                    for (strIndex = 0; strIndex <= strTemp.Length - 1; strIndex++)
                    {
                        if (strTemp[strIndex] == '=')
                        {
                            haveEqel = true;//默认参数
                            break;
                        }
                    }

                    if (!haveEqel) //当参数没有有默认值时
                    {
                        strItem = strTemp.Replace('\t', ' ');
                        //删除参数尾部多余的空格
                        strItem = strItem.Trim();
                        string[] strParamNameSplit = strItem.Split(new char[] { ' ' });
                        idata = new ITEMDATA();
                        idata.itemType = 2;
                        idata.itemText = strParamNameSplit[strParamNameSplit.Length - 1].Trim();
                    }
                    else //当参数有默认值时
                    {
                        string[] strParamNameSplit1 = strItem.Split(new char[] { '=' });
                        string strtemp = strParamNameSplit1[0].Trim();
                        strItem = strItem.Replace('\t', ' ');
                        string[] strparamNameSplit2 = strtemp.Split(new char[] { ' ' });
                        idata = new ITEMDATA();
                        idata.itemType = 2;
                        idata.itemText = strparamNameSplit2[strparamNameSplit2.Length - 1].Trim();
                    }

                    if (idata.itemText.Length > 0)
                    {
                        if (idata.itemText[0] == '*' || idata.itemText[0] == '&') //去掉参数前的* 和&
                            idata.itemText = idata.itemText.Remove(0, 1);
                    }

                    if (!idata.itemText.Equals(""))
                        _itemList.Add(idata);
                }
            }
            else
            {
                idata = new ITEMDATA();
                idata.itemType = 2;
                idata.itemText = "";
                _itemList.Add(idata);
            }

            //3.0 解析返回值类型
            if (strHeadSplit.Length >= 2)
            {
                idata = new ITEMDATA();
                idata.itemType = 3;
                idata.itemText = strHeadSplit[strHeadSplit.Length - 2].Trim();
                if (idata.itemText.Equals("WINAPI") ||
                   idata.itemText.Equals("__stdcall") ||
                   idata.itemText.Equals("FASTCALL") ||
                   idata.itemText.Equals("__fastcall") ||
                   idata.itemText.Equals("CDECL") ||
                   idata.itemText.Equals("__cdecl"))
                {
                    idata.itemText = strHeadSplit[strHeadSplit.Length - 3].Trim();
                }

                _itemList.Add(idata);
            }

            return (true);
        }

        public void CppChangeHistoryCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            EditPoint tempText;
            int iCurrentLineNumber = 0;
            int tempLineNumber = 0;
            int iLineLength = 0;
            string strFunText = "";
            int iItemIndex = 0;
            ITEMDATA idata = new ITEMDATA();
            int pcount = 0;
            string strSpace = "";
            int iSpaceIndex = 0;
            int strIndex = 0;
            string tempFunText;

            _itemList.Clear();
            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);

            //对函数的声明有多行的情况，以及每行后面有 //注释的情况作了处理，提高对函数的识别率
            //例如： int fun (int x,   // 变量x.. 
            //               int y ); //变量y.. 
            tempLineNumber = iCurrentLineNumber;
            tempText = _textDoc.StartPoint.CreateEditPoint();
            do
            {
                tempText.MoveToLineAndOffset(tempLineNumber, 1);
                iLineLength = tempText.LineLength;
                tempFunText = tempText.GetText(iLineLength);
                iLineLength = tempFunText.Length;
                if (0 == iLineLength)
                    continue;
                else if (iLineLength >= 2)
                {
                    for (strIndex = 0; strIndex <= iLineLength - 2; strIndex++)
                    {
                        if (tempFunText[strIndex] == '/' && tempFunText[strIndex + 1] == '/')
                        {
                            tempFunText = tempFunText.Substring(0, strIndex);
                            tempFunText = tempFunText.TrimEnd();
                            break;
                        }
                    }
                }

                strFunText = strFunText + tempFunText.TrimEnd();
                iLineLength = strFunText.Length;

                if (strFunText.Contains(")"))
                    break;

                tempLineNumber += 1;
            } while (true);

            if (iLineLength == 0)
                return;

            for (iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            //解析函数声明，获取函数返回值、函数名、函数参数
            bool bResult = ParseFunctionText(strFunText.Trim());
            if (!bResult)
            {
                _itemList.Clear();
                return;
            }

            outText.LineUp(1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength);

            //修改说明：判断是否为gFuncSeparater行，先要找到当前行的有效字符的起始位置，排除空格。
            if (strFunText.Length >= _FuncSeparater.Length && strFunText.IndexOf("/") >= 0)
            {
                if (strFunText.Substring(strFunText.IndexOf("/"), _FuncSeparater.Length) == _FuncSeparater)
                {
                    int iSepLine = outText.Line;
                    do
                    {
                        if (outText.Line == 1)
                            break;

                        outText.LineUp(1);
                        iLineLength = outText.LineLength;
                        strFunText = outText.GetText(iLineLength);

                        if ((strFunText.Length >= _FuncSeparater.Length) &&
                           (strFunText.Substring(0, _FuncSeparater.Length) == _FuncSeparater))
                            break;

                        //已有修改历史
                        if (strFunText.Length >= _ChangeHistory.Length &&
                            strFunText.Contains(_ChangeHistory))
                        {
                            string strNum = "1";
                            int iNum = 1;
                            string []strHistorySplit = strFunText.Split(new char[] { '：' });
                            if (null != strHistorySplit && strHistorySplit.Length >= 2)
                            {
                                strNum = strHistorySplit[strHistorySplit.Length - 1].Trim();
                                if (int.TryParse(strNum, out iNum))
                                    iNum = iNum + 1;
                            }                       

                            outText.MoveToLineAndOffset(outText.Line, strFunText.Length-strNum.Length+1);
                            outText.Delete(strNum.Length);
                            outText.Insert(iNum.ToString());

                            outText.MoveToLineAndOffset(iSepLine, 1);
                            outText.Insert(strSpace + "// " + iNum.ToString() + ".修改人：" + _Author + " " + DateTime.Now.ToString("yyyy-MM-dd") + "\n");
                            outText.Insert(strSpace + "//   修改问题：<简要说明所修改问题>" + "\n");

                            _textDoc.Selection.GotoLine(iCurrentLineNumber);
                            return;
                        }
                    } while (true);
                }
            }

            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            _textDoc.Selection.GotoLine(iCurrentLineNumber);

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 1:
                        {
                            string strFuncName = idata.itemText;
                            string tempStr = "";
                            for (int funNameLen = 1; funNameLen <= (_FuncSeparaterNum - 2 - idata.itemText.Trim().Length) / 2; funNameLen++)
                                tempStr = tempStr + "=";
                            if ((_FuncSeparaterNum - 2 - idata.itemText.Trim().Length) % 2 == 1)
                                outText.Insert(strSpace + "//" + tempStr + idata.itemText + "()" + tempStr.Substring(1) + "\n");
                            else
                                outText.Insert(strSpace + "//" + tempStr + idata.itemText + "()" + tempStr + "\n");

                            outText.Insert(strSpace + "// <对函数进行描述说明>" + "\n");
                            outText.Insert(strSpace + "//" + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 2:
                        {
                            if (pcount == 0)
                            {
                                if (idata.itemText.Length != 0)
                                    outText.Insert(strSpace + "// @param [in, out] " + idata.itemText + " <函数参数说明>" + "\n");
                            }
                            else
                            {
                                outText.Insert(strSpace + "// @param [in, out] " + idata.itemText + " <函数参数说明>" + "\n");
                            }

                            pcount = pcount + 1;
                        }
                        break;
                    case 3:
                        {
                            outText.Insert(strSpace + "//" + "\n");
                            outText.Insert(strSpace + "// @return" + " <返回值说明>" + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            outText.Insert(strSpace + "//" + "\n");
            outText.Insert(strSpace + "// @remark <函数特别说明>" + "\n");
            outText.Insert(strSpace + "//" + "\n");
            outText.Insert(strSpace + "// 修改历史：1" + "\n");
            outText.Insert(strSpace + "// 1.修改人：" + _Author + " " + DateTime.Now.ToString("yyyy-MM-dd") + "\n");
            outText.Insert(strSpace + "//   修改问题：<简要说明所修改问题>" + "\n");
            outText.Insert(strSpace + _FuncSeparater + "\n");
         
            _itemList.Clear();
            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        public void DocClassCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            int iCurrentLineNumber = 0;
            int iLineLength = 0;
            string strFunText;
            string[] splitStr;
            string[] splitClassNameStr;
            string tempStr = "";
            int classNameLen = 0;

            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength).TrimEnd();

            if (strFunText.Contains(":"))
            {
                splitClassNameStr = strFunText.Split(new char[] { ':' }); //有：号就是有继承，类的名字就在：的左边
                if (splitClassNameStr.Length > 1)
                {
                    strFunText = splitClassNameStr[0].TrimEnd();
                }
            }
            splitStr = strFunText.Split(new char[] { ' ' });

            string strSpace = "";
            for (int iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            for (classNameLen = 1; classNameLen <= (_FuncSeparaterNum - splitStr[splitStr.Length - 1].Trim().Length) / 2; classNameLen++)
                tempStr += "=";

            if ((_FuncSeparaterNum - splitStr[splitStr.Length - 1].Trim().Length) % 2 == 1)
            {
                outText.Insert(strSpace + "//" + tempStr + splitStr[splitStr.Length - 1].Trim() + tempStr.Substring(1) + "\n");
            }
            else
            {
                outText.Insert(strSpace + "//" + tempStr + splitStr[splitStr.Length - 1].Trim() + tempStr + "\n");
            }

            outText.Insert(strSpace + "/// @brief <对该类进行简单描述>" + "\n");
            outText.Insert(strSpace + "///" + "\n");
            outText.Insert(strSpace + "/// <对该类进行详细描述>" + "\n");
            outText.Insert(strSpace + _FuncSeparater + "\n");
            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }              

        public void DocFunctionCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            EditPoint tempText;
            int iCurrentLineNumber = 0;
            int tempLineNumber = 0;
            int iLineLength = 0;
            string strFunText = "";
            int iItemIndex = 0;
            ITEMDATA idata = new ITEMDATA();
            int pcount = 0;
            string strSpace = "";
            int iSpaceIndex = 0;
            int strIndex = 0;
            string tempFunText;
            string tempStr;
            int funNameLen = 0;

            _itemList.Clear();
            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);

            //对函数的声明有多行的情况，以及每行后面有 //注释的情况作了处理，提高对函数的识别率
            //例如： int fun (int x,   // 变量x.. 
            //               int y ); //变量y.. 
            tempLineNumber = iCurrentLineNumber;
            tempText = _textDoc.StartPoint.CreateEditPoint();
            do
            {
                tempText.MoveToLineAndOffset(tempLineNumber, 1);
                iLineLength = tempText.LineLength;
                tempFunText = tempText.GetText(iLineLength);
                iLineLength = tempFunText.Length;
                if (0 == iLineLength)
                    continue;
                else if (iLineLength >= 2)
                {
                    for (strIndex = 0; strIndex <= iLineLength - 2; strIndex++)
                    {
                        if (tempFunText[strIndex] == '/' && tempFunText[strIndex + 1] == '/')
                        {
                            tempFunText = tempFunText.Substring(0, strIndex);
                            tempFunText = tempFunText.TrimEnd();
                            break;
                        }
                    }
                }

                strFunText = strFunText + tempFunText.TrimEnd();
                iLineLength = strFunText.Length;

                if (strFunText.Contains(")"))
                    break;

                tempLineNumber += 1;
            } while (true);

            if (iLineLength == 0)
                return;

            for (iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            //解析函数声明，获取函数返回值、函数名、函数参数
            bool bResult = ParseFunctionText(strFunText.Trim());
            if (!bResult)
            {
                _itemList.Clear();
                return;
            }

            outText.LineUp(1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength);
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 1:
                        {
                            tempStr = "";
                            for (funNameLen = 1; funNameLen <= (_FuncSeparaterNum - 2 - idata.itemText.Trim().Length) / 2; funNameLen++)//处理注释分割线一行最后一行长度不一致
                                tempStr = tempStr + "=";

                            if ((_FuncSeparaterNum - 2 - idata.itemText.Trim().Length) % 2 == 1)
                                outText.Insert(strSpace + "//" + tempStr + idata.itemText + "()" + tempStr.Substring(1) + "\n");
                            else
                                outText.Insert(strSpace + "//" + tempStr + idata.itemText + "()" + tempStr + "\n");

                            outText.Insert(strSpace + "/// @brief <对函数进行概括说明>" + "\n");
                            outText.Insert(strSpace + "///" + "\n");
                            outText.Insert(strSpace + "/// <对函数进行详细说明>" + "\n");
                            outText.Insert(strSpace + "///" + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 2:
                        {
                            if (pcount == 0)
                            {
                                if (idata.itemText.Length != 0)
                                    outText.Insert(strSpace + "/// @param [in, out] " + idata.itemText + " <参数说明，[in, out]表示参数的传递方向，需要根据情况进行选择>" + "\n");
                            }
                            else
                            {
                                outText.Insert(strSpace + "/// @param [in, out] " + idata.itemText + " <参数说明，[in, out]表示参数的传递方向，需要根据情况进行选择>" + "\n");
                            }

                            pcount = pcount + 1;
                        }
                        break;
                    case 3:
                        {
                            outText.Insert(strSpace + "///" + "\n");
                            outText.Insert(strSpace + "/// @return" + " <返回值说明，可以配合使用DocGenList或DocGenTable>" + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            outText.Insert(strSpace + "///" + "\n");
            outText.Insert(strSpace + "/// @remark <函数特别说明>" + "\n");
            outText.Insert(strSpace + "///" + "\n");
            outText.Insert(strSpace + "/// @code" + "\n");
            outText.Insert(strSpace + "///      <在此添加示例代码>" + "\n");
            outText.Insert(strSpace + "/// @endcode" + "\n");
            outText.Insert(strSpace + "///" + "\n");
            outText.Insert(strSpace + "/// @sa <可以参考的类或函数，用空格分隔，函数名称后必须加()>" + "\n");
            outText.Insert(strSpace + _FuncSeparater + "\n");
            _itemList.Clear();
            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        public void DocMemberCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            int iCurrentLineNumber = 0;
            int iLineLength = 0;
            string strFunText;

            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength).TrimEnd();

            outText.MoveToLineAndOffset(iCurrentLineNumber, iLineLength + 1);
            outText.Insert("  ///< <成员变量说明>");
            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        public void DocGenListCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            int iCurrentLineNumber = 0;
            int iLineLength = 0;
            string strFunText;

            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength).TrimEnd();

            string strSpace = "";
            for (int iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            outText.Insert(strSpace + "/// - <List项描述>" + "\n");
            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        public void DocGenTableCallBack()
        {
            if (null == _textDoc)
                return;

            InputBox inputBox = new InputBox();
            int rowNum = 0;
            int colNum = 0;
            if (inputBox.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rowNum = inputBox.RowNum;
                colNum = inputBox.ColNum;
            }
            else
                return;

            EditPoint outText;
            int iCurrentLineNumber = 0;
            int iLineLength = 0;
            string strFunText;
            int colIndex = 0;

            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength);

            string strSpace = "";
            for (int iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            outText.Insert(strSpace + "///			|");
            for (colIndex = 1; colIndex <= colNum; colIndex++)
                outText.Insert("columnName         |");
            outText.Insert("\n");

            outText.Insert(strSpace + "///			|");
            for (colIndex = 1; colIndex <= colNum; colIndex++)
                outText.Insert("-------------------|");
            outText.Insert("\n");

            //输入行数
            for (int rowIndex = 1; rowIndex <= rowNum; rowIndex++)
            {
                outText.Insert(strSpace + "///			|");
                for (colIndex = 1; colIndex <= colNum; colIndex++)
                    outText.Insert("                   |");
                outText.Insert("\n");
            }

            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        public void DoNetChangeHistoryCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            EditPoint tempText;
            int iCurrentLineNumber = 0;
            int tempLineNumber = 0;
            int iLineLength = 0;
            string strFunText = "";
            int iItemIndex = 0;
            ITEMDATA idata = new ITEMDATA();
            int pcount = 0;
            string strSpace = "";
            int iSpaceIndex = 0;
            int strIndex = 0;
            string tempFunText;

            _itemList.Clear();
            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);

            //对函数的声明有多行的情况，以及每行后面有 //注释的情况作了处理，提高对函数的识别率
            //例如： int fun (int x,   // 变量x.. 
            //               int y ); //变量y.. 
            tempLineNumber = iCurrentLineNumber;
            tempText = _textDoc.StartPoint.CreateEditPoint();
            do
            {
                tempText.MoveToLineAndOffset(tempLineNumber, 1);
                iLineLength = tempText.LineLength;
                tempFunText = tempText.GetText(iLineLength);
                iLineLength = tempFunText.Length;
                if (0 == iLineLength)
                    continue;
                else if (iLineLength >= 2)
                {
                    for (strIndex = 0; strIndex <= iLineLength - 2; strIndex++)
                    {
                        if (tempFunText[strIndex] == '/' && tempFunText[strIndex + 1] == '/')
                        {
                            tempFunText = tempFunText.Substring(0, strIndex);
                            tempFunText = tempFunText.TrimEnd();
                            break;
                        }
                    }
                }

                strFunText = strFunText + tempFunText.TrimEnd();
                iLineLength = strFunText.Length;

                if (strFunText.Contains(")"))
                    break;

                tempLineNumber += 1;
            } while (true);

            if (iLineLength == 0)
                return;

            for (iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            //解析函数声明，获取函数返回值、函数名、函数参数
            bool bResult = ParseFunctionText(strFunText.Trim());
            if (!bResult)
            {
                _itemList.Clear();
                return;
            }

            outText.LineUp(1);
            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength);

            //修改说明：判断是否为gFuncSeparater行，先要找到当前行的有效字符的起始位置，排除空格。
            if (strFunText.Length >= _FuncSeparater.Length && strFunText.IndexOf("/") >= 0)
            {
                if (strFunText.Substring(strFunText.IndexOf("/"), _FuncSeparater.Length) == _FuncSeparater)
                {
                    int iSepLine = outText.Line;
                    do
                    {
                        if (outText.Line == 1)
                            break;

                        outText.LineUp(1);
                        iLineLength = outText.LineLength;
                        strFunText = outText.GetText(iLineLength);

                        if ((strFunText.Length >= _FuncSeparater.Length) &&
                           (strFunText.Substring(0, _FuncSeparater.Length) == _FuncSeparater))
                            break;

                        if (strFunText.Length >= _ChangeHistory.Length &&
                            strFunText.Contains(_ChangeHistory))
                        {
                            string strNum = "1";
                            int iNum = 1;
                            string[] strHistorySplit = strFunText.Split(new char[] { '：' });
                            if (null != strHistorySplit && strHistorySplit.Length >= 2)
                            {
                                strNum = strHistorySplit[strHistorySplit.Length - 1].Trim();
                                if (int.TryParse(strNum, out iNum))
                                    iNum = iNum + 1;
                            }

                            outText.MoveToLineAndOffset(outText.Line, strFunText.Length - strNum.Length + 1);
                            outText.Delete(strNum.Length);
                            outText.Insert(iNum.ToString());
                            outText.MoveToLineAndOffset(iSepLine, 1);
                            outText.Insert(strSpace + "// " + iNum.ToString() + ".修改人：" + _Author + " " + DateTime.Now.ToString("yyyy-MM-dd") + "\n");
                            outText.Insert(strSpace + "//   修改问题：<简要说明所修改问题>" + "\n");

                            _textDoc.Selection.GotoLine(iCurrentLineNumber);
                            return;
                        }
                    } while (true);
                }
            }

            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);
            _textDoc.Selection.GotoLine(iCurrentLineNumber);

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 1:
                        {
                            string strFuncName = idata.itemText;
                            string tempStr = "";
                            for (int funNameLen = 1; funNameLen <= (_FuncSeparaterNum - 2 - idata.itemText.Trim().Length) / 2; funNameLen++)
                                tempStr = tempStr + "=";
                            if ((_FuncSeparaterNum - 2 - idata.itemText.Trim().Length) % 2 == 1)
                                outText.Insert(strSpace + "//" + tempStr + idata.itemText + "()" + tempStr.Substring(1) + "\n");
                            else
                                outText.Insert(strSpace + "//" + tempStr + idata.itemText + "()" + tempStr + "\n");

                            outText.Insert(strSpace + "// <summary>" + "\n");
                            outText.Insert(strSpace + "// <对函数进行描述说明>" + "\n");
                            outText.Insert(strSpace + "// </summary>" + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 2:
                        {
                            if (pcount == 0)
                            {
                                if (idata.itemText.Length != 0)
                                    outText.Insert(strSpace + "// <param name=\"" + idata.itemText + "\"><对函数进行描述说明></param>  " + "\n");
                            }
                            else
                            {
                                outText.Insert(strSpace + "// <param name=\"" + idata.itemText + "\"><对函数进行描述说明></param>" + "\n");
                            }

                            pcount = pcount + 1;
                        }
                        break;
                    case 3:
                        {
                            outText.Insert(strSpace + "// <returns><返回值说明></returns> " + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            outText.Insert(strSpace + "//" + "\n");
            outText.Insert(strSpace + "// 修改历史：1" + "\n");
            outText.Insert(strSpace + "// 1.修改人：" + _Author + " " + DateTime.Now.ToString("yyyy-MM-dd") + "\n");
            outText.Insert(strSpace + "//   修改问题：<简要说明所修改问题>" + "\n");

            outText.Insert(strSpace + _FuncSeparater + "\n");
            _itemList.Clear();

            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }

        public void DoNetFunctionCallBack()
        {
            if (null == _textDoc)
                return;

            EditPoint outText;
            EditPoint tempText;
            int iCurrentLineNumber = 0;
            int tempLineNumber = 0;
            int iLineLength = 0;
            string strFunText = "";
            int iItemIndex = 0;
            ITEMDATA idata = new ITEMDATA();
            int pcount = 0;
            string strSpace = "";
            int iSpaceIndex = 0;
            int strIndex = 0;
            string tempFunText;

            _itemList.Clear();
            iCurrentLineNumber = _textDoc.Selection.CurrentLine;
            outText = _textDoc.StartPoint.CreateEditPoint();
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);

            //对函数的声明有多行的情况，以及每行后面有 //注释的情况作了处理，提高对函数的识别率
            //例如： int fun (int x,   // 变量x.. 
            //               int y ); //变量y.. 
            tempLineNumber = iCurrentLineNumber;
            tempText = _textDoc.StartPoint.CreateEditPoint();
            do
            {
                tempText.MoveToLineAndOffset(tempLineNumber, 1);
                iLineLength = tempText.LineLength;
                tempFunText = tempText.GetText(iLineLength);
                iLineLength = tempFunText.Length;
                if (0 == iLineLength)
                    continue;
                else if (iLineLength >= 2)
                {
                    for (strIndex = 0; strIndex <= iLineLength - 2; strIndex++)
                    {
                        if (tempFunText[strIndex] == '/' && tempFunText[strIndex + 1] == '/')
                        {
                            tempFunText = tempFunText.Substring(0, strIndex);
                            tempFunText = tempFunText.TrimEnd();
                            break;
                        }
                    }
                }

                strFunText = strFunText + tempFunText.TrimEnd();
                iLineLength = strFunText.Length;

                if (strFunText.Contains(")"))
                    break;

                tempLineNumber += 1;
            } while (true);

            if (iLineLength == 0)
                return;

            for (iSpaceIndex = 1; iSpaceIndex <= iLineLength; iSpaceIndex++)
            {
                if (strFunText[iSpaceIndex - 1] == '\t' ||
                    strFunText[iSpaceIndex - 1] == ' ')
                {
                    strSpace = strSpace + strFunText[iSpaceIndex - 1];
                }
                else
                    break;
            }

            //解析函数声明，获取函数返回值、函数名、函数参数
            bool bResult = ParseFunctionText(strFunText.Trim());
            if (!bResult)
            {
                _itemList.Clear();
                return;
            }

            iLineLength = outText.LineLength;
            strFunText = outText.GetText(iLineLength);
            outText.MoveToLineAndOffset(iCurrentLineNumber, 1);

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 1:
                        {
                            outText.Insert(strSpace + "/// <summary>" + "\n");
                            outText.Insert(strSpace + "/// <对函数进行描述说明>" + "\n");
                            outText.Insert(strSpace + "/// </summary>" + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            for (iItemIndex = 0; iItemIndex <= _itemList.Count - 1; iItemIndex++)
            {
                idata = _itemList[iItemIndex];
                switch (idata.itemType)
                {
                    case 2:
                        {
                            if (pcount == 0)
                            {
                                if (idata.itemText.Length != 0)
                                    outText.Insert(strSpace + "/// <param name=\"" + idata.itemText + "\"><对函数进行描述说明></param>  " + "\n");
                            }
                            else
                            {
                                outText.Insert(strSpace + "/// <param name=\"" + idata.itemText + "\"><对函数进行描述说明></param>" + "\n");
                            }

                            pcount = pcount + 1;
                        }
                        break;
                    case 3:
                        {
                            outText.Insert(strSpace + "/// <returns><返回值说明></returns> " + "\n");
                        }
                        break;
                    default:
                        break;
                }
            }

            _itemList.Clear();
            _textDoc.Selection.GotoLine(iCurrentLineNumber);
        }
    }

}
