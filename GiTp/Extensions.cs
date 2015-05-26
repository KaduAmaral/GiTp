using System;
using System.Collections.Generic;

/// <summary>
/// Summary description for Class1
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Retorna o intervalo do array entre os dois indices.
    /// ... Inclusive for start index, exclusive for end index.
    /// </summary>
    public static T[] Slice<T>(this T[] source, int start, int end = 0)
    {
        if (end == 0)
            end = source.Length;
        // Handles negative ends.
        if (end < 0)
        {
            end = source.Length + end;
        }
        int len = end - start;

        // Return new array.
        T[] res = new T[len];
        for (int i = 0; i < len; i++)
        {
            res[i] = source[i + start];
        }
        return res;
    }

    /// <summary>
    /// Altera todas as ocorrências de oldValue por newValue
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <returns>String</returns>
    public static string ReplaceAll(this String s, String oldValue, String newValue = "")
    {
        while (s.IndexOf(oldValue) != -1)
        {
            s = s.Replace(oldValue, newValue);
        }
        return s;
    }

    /// <summary>
    /// Adiciona uma margem a esquerda da string
    /// </summary>
    /// <param name="size">Tamanho da margem</param>
    /// <param name="c">Caracter de margem, opciona. (Padrão: Espaço em braco)</param>
    /// <returns></returns>
    public static string MarginLeft(this String s, Int32 size, Char c = ' ')
    {
        return (new String(c, size)) + s;
    }

    /// <summary>
    /// Adiciona uma margem a direita da string
    /// </summary>
    /// <param name="size">Tamanho da margem</param>
    /// <param name="c">Caracter de margem, opciona. (Padrão: Espaço em braco)</param>
    /// <returns></returns>
    public static string MarginRight(this String s, Int32 size, Char c = ' ')
    {
        return s + (new String(c, size));
    }
}