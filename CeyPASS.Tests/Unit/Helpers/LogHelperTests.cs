using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit.Helpers
{
    public class LogHelperTests
    {
        // ─── Escape ───────────────────────────────────────────────────────────

        [Fact]
        public void Escape_NullGirdi_NullDoner()
        {
            LogHelper.Escape(null).Should().BeNull();
        }

        [Fact]
        public void Escape_TemizGirdi_DeğişmezDoner()
        {
            LogHelper.Escape("merhaba").Should().Be("merhaba");
        }

        [Fact]
        public void Escape_TirnakIceren_EscapelanirDoner()
        {
            // say "hi" → say \"hi\"
            LogHelper.Escape("say \"hi\"").Should().Be("say \\\"hi\\\"");
        }

        [Fact]
        public void Escape_TersBolüIceren_EscapelanirDoner()
        {
            // a\b → a\\b
            LogHelper.Escape(@"a\b").Should().Be(@"a\\b");
        }
    }
}
