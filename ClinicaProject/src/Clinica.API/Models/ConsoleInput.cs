public static class ConsoleInput
{
    public static string LerCpfComMascara()
    {
        string cpf = "";
        Console.Write("CPF: ");
        int cursorInicioX = Console.CursorLeft;
        int cursorInicioY = Console.CursorTop;

        while (cpf.Length < 11)
        {
            var tecla = Console.ReadKey(true);

            if (tecla.Key == ConsoleKey.Enter) continue;

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (cpf.Length > 0)
                {
                    cpf = cpf.Substring(0, cpf.Length - 1);
                   
                    Console.SetCursorPosition(cursorInicioX, cursorInicioY);
                    Console.Write(new string(' ', 15)); 
                    
                    Console.SetCursorPosition(cursorInicioX, cursorInicioY);
                    ExibirCpfMascarado(cpf);
                }
                continue;
            }

            if (char.IsDigit(tecla.KeyChar))
            {
                cpf += tecla.KeyChar;
                Console.SetCursorPosition(cursorInicioX, cursorInicioY);
                ExibirCpfMascarado(cpf);
            }
        }

        Console.WriteLine(); 
        return cpf;
    }

    private static void ExibirCpfMascarado(string cpf)
    {
        for (int i = 0; i < cpf.Length; i++)
        {
            if (i == 3 || i == 6) Console.Write(".");
            if (i == 9) Console.Write("-");
            Console.Write(cpf[i]);
        }
    }
}