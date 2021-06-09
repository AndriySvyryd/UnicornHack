grammar UnicornExpression;

/*
 * Parser Rules
 */

expression
   : Expression=expression '.' Member=IDENTIFIER                     #accessExpression
   | <assoc=right> Left=expression '^' Right=expression              #powerExpression
   | Left=expression (ASTERISK | SLASH) Right=expression             #multiplicativeExpression
   | Left=expression (PLUS | MINUS) Right=expression                 #additiveExpression
   | Left=expression (LT | GT | LE | GE | EQ | NEQ) Right=expression #relationalExpression
   | Left=expression '||' Right=expression                           #conditionalOrExpression
   | Left=expression '&&' Right=expression                           #conditionalAndExpression
   | (PLUS | MINUS | EXCLAMATION)? Expression=primaryExpression      #unaryExpression
   | Condition=expression '?' True=expression ':' False=expression   #conditionalExpression
   ;

primaryExpression
   : Integer=INTEGER                                                 #integerLiteralExpression
   | Float=FLOAT                                                     #floatLiteralExpression
   | String=STRING_LITERAL                                           #stringLiteralExpression
   | INFINITY                                                        #constantExpression
   | '$' Variable=IDENTIFIER                                         #variableExpression
   | Function=IDENTIFIER '(' expression? (',' expression)* ')'       #invocationExpression
   | '(' Expression=expression ')'                                   #parenthesizedExpression
   ;

/*
 * Lexer Rules
 */

LT
   : '<'
   ;

GT
   : '>'
   ;

LE
   : '<='
   ;

GE
   : '>='
   ;

EQ
   : '=='
   ;

NEQ
   : '!='
   ;

EXCLAMATION
   : '!'
   ;

ASTERISK
   : '*'
   ;

SLASH
   : '/'
   ;

PLUS
   : '+'
   ;

MINUS
   : '-'
   ;

POW
   : '^'
   ;

FLOAT
   : INTEGER '.' INTEGER
   ;

INTEGER
   : DIGIT+
   ;

fragment DIGIT
   : [0-9]
   ;

INFINITY
   : 'Infinity'
   ;

IDENTIFIER
   : IDENTIFIER_START_CHARACTER IDENTIFIER_PART_CHARACTER*
   ;

fragment IDENTIFIER_START_CHARACTER
   : LETTER
   | '_'
   ;

fragment IDENTIFIER_PART_CHARACTER
   : LETTER
   | DIGIT
   ;

fragment LETTER
    : [a-zA-Z]
    ;

STRING_LITERAL
   : '\'' ( ~ '\'' | '\'\'' )* '\''
   ;

WS
   : [ \r\n\t]+ -> skip
   ;