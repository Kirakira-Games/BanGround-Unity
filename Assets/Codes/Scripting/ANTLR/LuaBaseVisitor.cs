//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Lua.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ILuaVisitor{Result}"/>,
/// which can be extended to create a visitor which only needs to handle a subset
/// of the available methods.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
public partial class LuaBaseVisitor<Result> : AbstractParseTreeVisitor<Result>, ILuaVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.chunk"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitChunk([NotNull] LuaParser.ChunkContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.block"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBlock([NotNull] LuaParser.BlockContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitStat([NotNull] LuaParser.StatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.attnamelist"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitAttnamelist([NotNull] LuaParser.AttnamelistContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.attrib"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitAttrib([NotNull] LuaParser.AttribContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.retstat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitRetstat([NotNull] LuaParser.RetstatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.label"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLabel([NotNull] LuaParser.LabelContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.funcname"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitFuncname([NotNull] LuaParser.FuncnameContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.varlist"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitVarlist([NotNull] LuaParser.VarlistContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.namelist"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNamelist([NotNull] LuaParser.NamelistContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.explist"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitExplist([NotNull] LuaParser.ExplistContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.exp"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitExp([NotNull] LuaParser.ExpContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.prefixexp"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitPrefixexp([NotNull] LuaParser.PrefixexpContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.functioncall"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitFunctioncall([NotNull] LuaParser.FunctioncallContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.varOrExp"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitVarOrExp([NotNull] LuaParser.VarOrExpContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.var"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitVar([NotNull] LuaParser.VarContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.varSuffix"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitVarSuffix([NotNull] LuaParser.VarSuffixContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.nameAndArgs"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNameAndArgs([NotNull] LuaParser.NameAndArgsContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.args"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitArgs([NotNull] LuaParser.ArgsContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.functiondef"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitFunctiondef([NotNull] LuaParser.FunctiondefContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.funcbody"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitFuncbody([NotNull] LuaParser.FuncbodyContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.parlist"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitParlist([NotNull] LuaParser.ParlistContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.tableconstructor"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitTableconstructor([NotNull] LuaParser.TableconstructorContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.fieldlist"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitFieldlist([NotNull] LuaParser.FieldlistContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.field"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitField([NotNull] LuaParser.FieldContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.fieldsep"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitFieldsep([NotNull] LuaParser.FieldsepContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorOr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorOr([NotNull] LuaParser.OperatorOrContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorAnd"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorAnd([NotNull] LuaParser.OperatorAndContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorComparison"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorComparison([NotNull] LuaParser.OperatorComparisonContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorStrcat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorStrcat([NotNull] LuaParser.OperatorStrcatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorAddSub"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorAddSub([NotNull] LuaParser.OperatorAddSubContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorMulDivMod"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorMulDivMod([NotNull] LuaParser.OperatorMulDivModContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorBitwise"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorBitwise([NotNull] LuaParser.OperatorBitwiseContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorUnary"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorUnary([NotNull] LuaParser.OperatorUnaryContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.operatorPower"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOperatorPower([NotNull] LuaParser.OperatorPowerContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.number"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNumber([NotNull] LuaParser.NumberContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by <see cref="LuaParser.string"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitString([NotNull] LuaParser.StringContext context) { return VisitChildren(context); }
}
