// AutoItemDetect4.0.cpp: 定义应用程序的入口点。
//

#include "AutoItemDetect4.0.h"

#include <stdio.h>
#include <algorithm>
#include <cmath>
#include <vector>
#include <string>
#include <cstring>
#include <atlbase.h>
#include <Windows.h>

#include <vector>


using namespace std; // 写一个c++小项目

// 可以查询某个地方的注册表，只需要传入不同的路径即可
// 效果还是ok的
// 之后用链表写入QT页面即可

// 定义了两个小的全局变量，专门用于数据处理

void OnBnClickedQuery() {
    HKEY hKEY; // 有关的hKEY，在查询结束时关闭

    // 先从HKEY_CURRENT_USER开始寻找
    // Run键的值

    // 打开与路径dataSetRun相关的hKEY
    LPCTSTR dataSetRun = _T(
        "Software\\Microsoft\\Windows\\CurrentVersion\\Run"
    );
    // RunOnce是空的，估计查询不到什么
    LPCTSTR dataSetRunOnce = _T(
        "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
    );

    if (ERROR_SUCCESS == ::RegOpenKeyExA(
        HKEY_CURRENT_USER, dataSetRun, 0, KEY_READ, &hKEY
    )) {
        char dwValue[1024]; // 数据是REG_SZ类型，使用char数组
        DWORD dwSzType = REG_SZ;
        DWORD dwSize = sizeof(dwSzType);

        // 下面这行展示了核心的查找函数
        // if(::RegQueryValueEx(hKEY, _T()))
        // 不需要查询下面的内容的
        cout << dwValue << endl;

        int i = 0;
        // 一些数据缓冲区
        // 这个就是char，只不过换了一个名字，保证通用性

        WCHAR
        TCHAR szData[MAX_PATH] = { 0 };
        TCHAR subKey[MAX_PATH] = { 0 };

        for (;; ++i) {
            // 清空缓冲区
            ZeroMemory(subKey, sizeof(subKey));
            ZeroMemory(szData, sizeof(szData));

            DWORD sizeKey = sizeof(subKey);
            DWORD szCbData = sizeof(szData);

            DWORD dwType = REG_SZ;

            // 以上操作暂时都看不懂

            if (RegEnumValue(hKEY, i, subKey, &sizeKey, NULL, &dwType, \
                (LPBYTE)szData, &szCbData) != ERROR_SUCCESS) {
                printf("search for register table failure! %d \n", i);
                break;
            }
            else {
                cout << subKey << endl;
                cout << szData << endl;
            }
        }

        ::RegCloseKey(hKEY);
    }
    else {
        printf("open failure!");
        printf("\n %d \n", ::RegOpenKeyExA(
            HKEY_CURRENT_USER, dataSetRun, 0, KEY_READ, &hKEY
        ));
    }
    return;
}

// 现在暂时编译过去了，我真的是醉了  
// CMake编写真的是不舒服
int main(void) {
    int val = 0;
    while (val < 10) {
        printf("%d \n", val);
        val++;
    }
    // 暂时的环境配置算是成功了
    // 某些stl库还不知道要如何导入呢？需要适当了解一下了

    // 查询注册表的尝试 try
    OnBnClickedQuery();

    return 0;
}

