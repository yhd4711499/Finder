// Finder.AlgorithmsTest.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "stdafx.h"
#include "iostream"
#include "../Finder.UnmanagedAlgorithms/Header.h"

int _tmain(int argc, _TCHAR* argv[])
{
	int size = getDeltaMapSize();
	int* map = new int[size];
	TCHAR* source = _T("����ֱ�ӵ��þ�̬������ķ������ʵ");
	TCHAR* pattern = _T("����");
	createDeltaMap(pattern, map);
	std::cout << contains(source, lstrlenW(source), pattern, lstrlenW(pattern), map);
	return 0;
}

