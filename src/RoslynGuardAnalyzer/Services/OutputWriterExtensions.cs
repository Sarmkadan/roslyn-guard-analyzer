using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Services
{
    /// <summary>
    /// Extension methods for <see cref="OutputWriter"/>.
    /// </summary>
    public static class OutputWriterExtensions
    {
        /// <summary>
        /// Returns the supported output formats as a read‑only list.
        /// </summary>
        /// <param name="writer">The <see cref="OutputWriter"/> instance.</param>
        /// <returns>A read‑only list of format strings.</returns>
        public static IReadOnlyList<string> GetSupportedFormatsList(this OutputWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            // Materialise the enumerable to a list and expose it as read‑only.
            return writer.GetSupportedFormats().ToList().AsReadOnly();
        }

        /// <summary>
        /// Throws a <see cref="NotSupportedException"/> if the specified format is not supported.
        /// </summary>
        /// <param name="writer">The <see cref="OutputWriter"/> instance.</param>
        /// <param name="format">The format to validate.</param>
        public static void EnsureFormatSupported(this OutputWriter writer, string format)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (format == null) throw new ArgumentNullException(nameof(format));

            if (!writer.IsFormatSupported(format))
            {
                throw new NotSupportedException($"The format '{format}' is not supported by this OutputWriter.");
            }
        }

        /// <summary>
        /// Executes an asynchronous write operation only if the requested format is supported.
        /// </summary>
        /// <param name="writer">The <see cref="OutputWriter"/> instance.</param>
        /// <param name="format">The format to be used for the write operation.</param>
        /// <param name="writeAsync">A delegate that performs the actual write operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task WriteIfSupportedAsync(this OutputWriter writer, string format, Func<Task> writeAsync)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (writeAsync == null) throw new ArgumentNullException(nameof(writeAsync));

            writer.EnsureFormatSupported(format);
            await writeAsync().ConfigureAwait(false);
        }
    }
}
