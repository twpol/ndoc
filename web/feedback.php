<html>
<head>

	<title>hi.</title>

</head>
<body>

	<?php if ($REQUEST_METHOD == 'GET') { ?>

		<h1>GET</h1>

		<form method="POST" action="<?php echo $PHP_SELF ?>" enctype="application/x-www-form-urlencoded">
			<table>
				<tr><td align="right" valign="top">from:</td><td><input name="from"></td></tr>
				<tr><td align="right" valign="top">subject:</td><td><input name="subject"></td></tr>
				<tr><td align="right" valign="top">content:</td><td><textarea name="content" rows="8"></textarea></td></tr>
				<tr><td colspan="2" align="right"><input type="submit"></td></tr>
			</table>
		</form>

	<?php } else if ($REQUEST_METHOD == 'POST') { ?>

		<h1>POST</h1>

		<p><b>From</b>: <?php echo $from ?><br>
		<b>Subject</b>: <?php echo $subject ?></p>

		<p><?php echo $content ?></p>

		<?php mail('jason@injektilo.org', $subject, $content, 'From: ' . $from) ?>

		<p>Your message has been sent.</p>

	<?php } else { ?>

		<h1><?php echo $REQUEST_METHOD ?></h1>

		<p>This is an unsupported method.</p>

	<?php } ?>

</body>
</html>
