lexer grammar MatlabLexer;

// Ключевые слова
IF        : 'if';
ELSEIF    : 'elseif';
ELSE      : 'else';
END       : 'end';
FOR       : 'for';
WHILE     : 'while';
FUNCTION  : 'function';
RETURN    : 'return';
BREAK     : 'break';
CONTINUE  : 'continue';
TRUE      : 'true';
FALSE     : 'false';

// Идентификаторы и числа
ID        : [a-zA-Z_][a-zA-Z0-9_]* ;
INT       : [0-9]+ ;
FLOAT     : [0-9]+ '.' [0-9]* (('e'|'E') ('+'|'-')? [0-9]+)?
          | '.' [0-9]+ (('e'|'E') ('+'|'-')? [0-9]+)?
          ;
STRING    : '\'' (~['\r\n] | '\'\'')* '\'' ;

// Операторы
PLUS      : '+';
MINUS     : '-';
STAR      : '*';
SLASH     : '/';
CARET     : '^';
EQ        : '==';
NEQ       : '~=';
LT        : '<';
GT        : '>';
LEQ       : '<=';
GEQ       : '>=';
AND       : '&&';
OR        : '||';
NOT       : '~';
ASSIGN    : '=';

// Разделители
LPAREN    : '(';
RPAREN    : ')';
LBRACK    : '[';
RBRACK    : ']';
COMMA     : ',';
SEMICOLON : ';';
COLON     : ':';
DOT       : '.';
NEWLINE : '\r'? '\n'  ;

// Пропускаемые токены
COMMENT   : '%' ~[\r\n]* -> skip ;
BLOCKCOMMENT : '%{' .*? '%}' -> skip ;
WS        : [ \t]+ -> skip ;