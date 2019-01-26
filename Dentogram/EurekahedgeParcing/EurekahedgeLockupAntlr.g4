grammar EurekahedgeLockupAntlr;

/*
 * Parser Rules
 */

 general
	: (generalAtom	| .)+
;
 generalAtom
	: yesNo
	| percent
	| softHard
	| lockupDuration
 ;

 percent
	: PERCENT
 ;

 number
	: NUMBER
	| BORDER WORDNUMBERVALUE BORDER
 ;

 yesNo
	: YES
	| NO
 ;

 softHard
	: SOFT
	| HARD
 ;

 lockupDuration
	: number PLUS? SEPARATOR period
	| period SEPARATOR number
	| number period
 ;

 
period
	: YEAR
	| QUARTER
	| month
	| WEEK
	| DAY
;

month
	: MONTH
	| border 'm' border
;

border
   : ~('a'..'z')
;

/*
 * Lexer Rules
 */

YEAR
	: 'calendar year'
	| 'year' 's'?
	| 'yr' 's'?
	| 'annual'
;

QUARTER
	: 'quarter' ('ly'|'s')?
;

MONTH
	: 'calendar months'
	| 'month' ('ly'|'s')?
	| 'moth' 's'?
	| 'mth' 's'?
;

WEEK
	: 'week' 's'?
;

DAY
	: 'day' 's'?
;

NO
	: 'no'
;

YES
	: 'yes'
;

SOFT
	: 'soft' (SEPARATOR 'lock' (SEPARATOR 'up')?)?
;

HARD
	: 'hard' (SEPARATOR 'lock' (SEPARATOR 'up')?)?
;

WORDNUMBERVALUE
	: '1st'
	| 'first'
	| 'i'
	| 'one'
	| 'two'
	| 'six'
	| 'nine'
;

PERCENT
	: NUMBER '%'
;

NUMBER
   : [0-9] + ('.' [0-9] +)?
;

SEPARATOR
	: [ -]
;

PLUS
	: '+'
;

WS
   : [\r\n\t] + -> skip
;
