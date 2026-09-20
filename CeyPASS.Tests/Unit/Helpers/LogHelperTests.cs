using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit.Helpers
{
    /// <summary>
    /// Log/JSON metinlerinde tırnak ve ters eğik çizgi kaçışlama.
    /// </summary>
    public class LogHelperTests
    {
        // ─── Escape ───────────────────────────────────────────────────────────

        /// <summary>
        /// NullGirdi nulldöner
        /// </summary>
        [Fact]
        public void Escape_NullGirdi_NullDoner()
        {
            LogHelper.Escape(null).Should().BeNull();
        }

        /// <summary>
        /// TemizGirdi Değişmezdöner
        /// </summary>
        [Fact]
        public void Escape_TemizGirdi_DeğişmezDoner()
        {
            LogHelper.Escape("merhaba").Should().Be("merhaba");
        }

        /// <summary>
        /// TirnakIceren Escapelanirdöner
        /// </summary>
        [Fact]
        public void Escape_TirnakIceren_EscapelanirDoner()
        {
            // say "hi" → say \"hi\"
            LogHelper.Escape("say \"hi\"").Should().Be("say \\\"hi\\\"");
        }

        /// <summary>
        /// TersBolüIceren Escapelanirdöner
        /// </summary>
        [Fact]
        public void Escape_TersBolüIceren_EscapelanirDoner()
        {
            // a\b → a\\b
            LogHelper.Escape(@"a\b").Should().Be(@"a\\b");
        }
    }
}
