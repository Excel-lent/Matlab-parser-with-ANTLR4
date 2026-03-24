parser grammar MatlabParser;
options { tokenVocab = MatlabLexer; }

// Точка входа
program
    : (statement | NEWLINE)* EOF
    ;

statement
    : functionDecl
    | ifStatement
    | forStatement
    | whileStatement
    | assignStatement (SEMICOLON | NEWLINE) NEWLINE*
    | expression      (SEMICOLON | NEWLINE) NEWLINE*
    | RETURN          (SEMICOLON | NEWLINE) NEWLINE*
    | BREAK           (SEMICOLON | NEWLINE) NEWLINE*
    | CONTINUE        (SEMICOLON | NEWLINE) NEWLINE*
    ;

// Объявление функции
functionDecl
    : FUNCTION (returnVars '=')? ID '(' paramList? ')' NEWLINE+
        statement*
      END NEWLINE*
    ;

returnVars
    : ID
    | '[' ID (',' ID)* ']'
    ;

paramList
    : ID (',' ID)*
    ;

// Управляющие конструкции
ifStatement
    : IF expression NEWLINE+
        statement*
      (ELSEIF expression NEWLINE+ statement*)*
      (ELSE NEWLINE+ statement*)?
      END NEWLINE*
    ;

forStatement
    : FOR ID '=' expression NEWLINE+
        statement*
      END NEWLINE*
    ;

whileStatement
    : WHILE expression NEWLINE+
        statement*
      END NEWLINE*
    ;

// Присваивание
assignStatement
    : lvalue '=' expression
    ;

lvalue
    : ID
    | ID '(' argList ')'   // индексирование
    ;

// Выражения (с приоритетами)
expression
    : expression ('||') expression                   # orExpr
    | expression ('&&') expression                   # andExpr
    | expression ('==' | '~=') expression            # eqExpr
    | expression ('<' | '>' | '<=' | '>=') expression # relExpr
    | expression (':') expression                    # rangeExpr
    | expression ('+' | '-') expression              # addExpr
    | expression ('*' | '/') expression              # mulExpr
    | expression ('^') expression                    # powExpr
    | '~' expression                                 # notExpr
    | '-' expression                                 # unaryMinus
    | primary                                        # primaryExpr
    ;

primary
    : INT
    | FLOAT
    | STRING
    | TRUE
    | FALSE
    | ID
    | ID '(' argList? ')'     // вызов функции / индексирование
    | '[' matrixRows ']'      // матрица
    | '(' expression ')'
    ;

argList
    : expression (',' expression)*
    ;

matrixRows
    : matrixRow (';' matrixRow)*
    ;

matrixRow
    : expression (',' expression)*
    ;